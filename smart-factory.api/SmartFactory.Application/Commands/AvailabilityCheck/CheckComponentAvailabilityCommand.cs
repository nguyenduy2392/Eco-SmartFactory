using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.AvailabilityCheck;

/// <summary>
/// Command to check component availability (not PO-based)
/// Checks if a specific part with processing type can be produced
/// </summary>
public class CheckComponentAvailabilityCommand : IRequest<AvailabilityCheckResult>
{
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public int Quantity { get; set; }
    public Guid CustomerId { get; set; } // Filter materials by customer
}

public class CheckComponentAvailabilityCommandHandler : IRequestHandler<CheckComponentAvailabilityCommand, AvailabilityCheckResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CheckComponentAvailabilityCommandHandler> _logger;

    public CheckComponentAvailabilityCommandHandler(
        ApplicationDbContext context,
        ILogger<CheckComponentAvailabilityCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AvailabilityCheckResult> Handle(CheckComponentAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // Validate Part exists and load with Product
        var part = await _context.Parts
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PartId, cancellationToken);

        if (part == null)
        {
            throw new Exception($"Part with ID {request.PartId} not found");
        }

        // Validate ProcessingType exists
        var processingType = await _context.ProcessingTypes
            .FirstOrDefaultAsync(pt => pt.Id == request.ProcessingTypeId, cancellationToken);

        if (processingType == null)
        {
            throw new Exception($"ProcessingType with ID {request.ProcessingTypeId} not found");
        }

        if (request.Quantity <= 0)
        {
            throw new Exception("Quantity must be > 0");
        }

        var result = new AvailabilityCheckResult
        {
            PartId = request.PartId,
            ProcessingTypeId = request.ProcessingTypeId,
            Quantity = request.Quantity,
            CheckedAt = DateTime.UtcNow,
            OverallStatus = "PASS"
        };

        // Use customerId from request (required)
        var customerId = request.CustomerId;

        // Check if ACTIVE BOM exists for (Part + ProcessingType) and load details
        var activeBOM = await _context.ProcessBOMs
            .Where(b => b.PartId == request.PartId 
                && b.ProcessingTypeId == request.ProcessingTypeId 
                && b.Status == "ACTIVE")
            .Include(b => b.BOMDetails)
            .FirstOrDefaultAsync(cancellationToken);

        var detail = new PartAvailabilityDetail
        {
            PartId = part.Id,
            PartCode = part.Code,
            PartName = part.Name,
            ProcessingType = processingType.Code,
            ProcessingTypeName = processingType.Name,
            RequiredQuantity = request.Quantity,
            BOMVersion = activeBOM?.Version,
            HasActiveBOM = activeBOM != null
        };

        // If no BOM, mark as CRITICAL
        if (activeBOM == null)
        {
            detail.CanProduce = false;
            detail.Severity = "CRITICAL";
            result.OverallStatus = "FAIL";
        }
        else
        {
            // Check material availability for each BOM detail
            bool hasShortage = false;
            bool hasWarning = false;

            foreach (var bomDetail in activeBOM.BOMDetails.OrderBy(d => d.SequenceOrder))
            {
                var materialDetail = new MaterialAvailabilityDetail
                {
                    MaterialCode = bomDetail.MaterialCode,
                    MaterialName = bomDetail.MaterialName,
                    Unit = bomDetail.Unit,
                    QuantityPerUnit = bomDetail.QuantityPerUnit,
                    ScrapRate = bomDetail.ScrapRate
                };

                // Calculate required quantity: Quantity × QuantityPerUnit × (1 + ScrapRate)
                materialDetail.RequiredQuantity = request.Quantity * bomDetail.QuantityPerUnit * (1 + bomDetail.ScrapRate);

                // Find material in warehouse - only search in the specified customer's materials
                var material = await _context.Materials
                    .Include(m => m.Customer)
                    .FirstOrDefaultAsync(m => m.Code == bomDetail.MaterialCode 
                        && m.CustomerId == customerId 
                        && m.IsActive, cancellationToken);

                if (material != null)
                {
                    materialDetail.MaterialFound = true;
                    materialDetail.AvailableQuantity = material.CurrentStock;
                    materialDetail.CustomerId = material.CustomerId;
                    materialDetail.CustomerName = material.Customer?.Name;
                }
                else
                {
                    materialDetail.MaterialFound = false;
                    materialDetail.AvailableQuantity = 0;
                }

                // Calculate shortage
                materialDetail.Shortage = Math.Max(0, materialDetail.RequiredQuantity - materialDetail.AvailableQuantity);

                // Determine severity
                if (materialDetail.Shortage > 0)
                {
                    materialDetail.Severity = "CRITICAL";
                    hasShortage = true;
                }
                else if (materialDetail.AvailableQuantity < materialDetail.RequiredQuantity * 1.1m)
                {
                    // Less than 10% buffer
                    materialDetail.Severity = "WARNING";
                    hasWarning = true;
                }
                else
                {
                    materialDetail.Severity = "OK";
                }

                detail.MaterialDetails.Add(materialDetail);
            }

            // Determine part-level severity and canProduce
            if (hasShortage)
            {
                detail.CanProduce = false;
                detail.Severity = "CRITICAL";
                result.OverallStatus = "FAIL";
            }
            else if (hasWarning)
            {
                detail.CanProduce = true;
                detail.Severity = "WARNING";
                if (result.OverallStatus == "PASS")
                {
                    result.OverallStatus = "WARNING";
                }
            }
            else
            {
                detail.CanProduce = true;
                detail.Severity = "OK";
            }
        }

        result.PartDetails.Add(detail);

        _logger.LogInformation("Component availability check: Part {PartCode} ({ProcessingType}): Quantity={Quantity}, CustomerId={CustomerId}, CanProduce={CanProduce}, Severity={Severity}, MaterialCount={MaterialCount}",
            part.Code, processingType.Code, request.Quantity, customerId, detail.CanProduce, detail.Severity, detail.MaterialDetails.Count);

        return result;
    }
}


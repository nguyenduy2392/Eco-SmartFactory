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
        // Validate Part exists
        var part = await _context.Parts
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

        // Check if ACTIVE BOM exists for (Part + ProcessingType)
        var activeBOM = await _context.ProcessBOMs
            .Where(b => b.PartId == request.PartId 
                && b.ProcessingTypeId == request.ProcessingTypeId 
                && b.Status == "ACTIVE")
            .FirstOrDefaultAsync(cancellationToken);

        // Determine availability and severity
        string severity;
        bool canProduce = activeBOM != null;
        
        if (!canProduce)
        {
            severity = "CRITICAL";
            result.OverallStatus = "FAIL";
        }
        else
        {
            severity = "OK";
        }

        var detail = new PartAvailabilityDetail
        {
            PartId = part.Id,
            PartCode = part.Code,
            PartName = part.Name,
            ProcessingType = processingType.Code,
            ProcessingTypeName = processingType.Name,
            RequiredQuantity = request.Quantity,
            CanProduce = canProduce,
            Severity = severity,
            BOMVersion = activeBOM?.Version,
            HasActiveBOM = canProduce
        };

        result.PartDetails.Add(detail);

        _logger.LogInformation("Component availability check: Part {PartCode} ({ProcessingType}): Quantity={Quantity}, CanProduce={CanProduce}, Severity={Severity}",
            part.Code, processingType.Code, request.Quantity, canProduce, severity);

        return result;
    }
}


using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.AvailabilityCheck;

/// <summary>
/// Command to check part availability for production planning
/// PHASE 1: Availability Check Logic
/// Purpose: Decide whether PMC is allowed to plan production
/// 
/// Data sources:
/// - PO Operations (contract quantity per part)
/// - Process BOM (ACTIVE) - to verify if part can be produced
/// 
/// Calculation:
/// Required_Qty = Planned_Qty × PO_Operation_Quantity (per part)
/// Available: Check if ACTIVE BOM exists for (Part + ProcessingType)
/// - Has ACTIVE BOM → Can produce → OK
/// - No ACTIVE BOM → Cannot produce → CRITICAL
/// 
/// Result rules:
/// - No ACTIVE BOM → FAIL (CRITICAL)
/// - Has ACTIVE BOM → PASS
/// 
/// IMPORTANT: Availability check MUST NOT:
/// - Change inventory
/// - Create production data
/// - Affect pricing
/// </summary>
public class CheckMaterialAvailabilityCommand : IRequest<AvailabilityCheckResult>
{
    public Guid PurchaseOrderId { get; set; }
    public int PlannedQuantity { get; set; }
}

public class CheckMaterialAvailabilityCommandHandler : IRequestHandler<CheckMaterialAvailabilityCommand, AvailabilityCheckResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CheckMaterialAvailabilityCommandHandler> _logger;

    public CheckMaterialAvailabilityCommandHandler(
        ApplicationDbContext context,
        ILogger<CheckMaterialAvailabilityCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AvailabilityCheckResult> Handle(CheckMaterialAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // PHASE 1: Validate PO exists and is APPROVED_FOR_PMC
        var po = await _context.PurchaseOrders
            .Include(p => p.POOperations)
                .ThenInclude(op => op.Part)
            .Include(p => p.POOperations)
                .ThenInclude(op => op.ProcessingType)
            .Include(p => p.MaterialBaselines)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

        if (po == null)
        {
            throw new Exception($"PO with ID {request.PurchaseOrderId} not found");
        }

        // PHASE 1 Rule: Only APPROVED PO version can be used for availability check
        if (po.Status != "APPROVED_FOR_PMC")
        {
            throw new Exception($"PO {po.PONumber} is not APPROVED_FOR_PMC. Only approved PO versions can be used for availability check. Current status: {po.Status}");
        }

        if (request.PlannedQuantity <= 0)
        {
            throw new Exception("Planned quantity must be > 0");
        }

        var result = new AvailabilityCheckResult
        {
            PurchaseOrderId = po.Id,
            PlannedQuantity = request.PlannedQuantity,
            CheckedAt = DateTime.UtcNow,
            OverallStatus = "PASS"
        };

        // Step 1: Check availability for each part in PO Operations
        foreach (var operation in po.POOperations)
        {
            // Skip operations without PartId (e.g., LAP_RAP operations that don't require parts)
            if (!operation.PartId.HasValue)
            {
                continue;
            }

            // Required quantity = Planned_Qty × PO_Operation_Quantity
            var requiredQty = request.PlannedQuantity * operation.Quantity;

            // Get ACTIVE BOM for this (Part + ProcessingType)
            var activeBOM = await _context.ProcessBOMs
                .Where(b => b.PartId == operation.PartId.Value
                    && b.ProcessingTypeId == operation.ProcessingTypeId 
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
                PartId = operation.PartId ?? Guid.Empty,
                PartCode = operation.Part?.Code ?? string.Empty,
                PartName = operation.Part?.Name ?? string.Empty,
                ProcessingType = operation.ProcessingType.Code,
                ProcessingTypeName = operation.ProcessingType.Name,
                RequiredQuantity = requiredQty,
                CanProduce = canProduce,
                Severity = severity,
                BOMVersion = activeBOM?.Version,
                HasActiveBOM = canProduce
            };

            result.PartDetails.Add(detail);

            _logger.LogInformation("Part {PartCode} ({ProcessingType}): Required={Required}, CanProduce={CanProduce}, Severity={Severity}",
                operation.Part.Code, operation.ProcessingType.Code, requiredQty, canProduce, severity);
        }

        if (!result.PartDetails.Any())
        {
            _logger.LogWarning("No PO Operations found for PO {PONumber}. Cannot perform availability check.", po.PONumber);
            result.OverallStatus = "WARNING";
            return result;
        }

        _logger.LogInformation("Availability check for PO {PONumber}: Overall status = {Status}, Parts checked = {Count}",
            po.PONumber, result.OverallStatus, result.PartDetails.Count);

        return result;
    }
}








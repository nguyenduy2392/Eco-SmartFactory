using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.AvailabilityCheck;

/// <summary>
/// Command to check material availability for production planning
/// PHASE 1: Availability Check Logic
/// Purpose: Decide whether PMC is allowed to plan production
/// 
/// Data sources:
/// - PO Operations (contract quantity)
/// - Process BOM (ACTIVE)
/// - PO Material Baseline (NHAP_NGUYEN_VAT_LIEU)
/// - Inventory on-hand quantity
/// 
/// Calculation:
/// Required_Qty = Planned_Qty × BOM_Qty × (1 + Scrap_Rate)
/// Available_Qty = Inventory_Qty + PO_Material_Baseline_Qty
/// Shortage = Required_Qty - Available_Qty
/// 
/// Result rules:
/// - Shortage > 0 → FAIL (CRITICAL)
/// - Available_Qty < Required_Qty × 1.1 → WARNING
/// - Else → PASS
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

        // Dictionary to track material requirements
        var materialRequirements = new Dictionary<string, MaterialRequirement>();

        // Step 1: Calculate required materials from BOM for each PO Operation
        foreach (var operation in po.POOperations)
        {
            // Get ACTIVE BOM for this (Part + ProcessingType)
            var activeBOM = await _context.ProcessBOMs
                .Include(b => b.BOMDetails)
                .Where(b => b.PartId == operation.PartId 
                    && b.ProcessingTypeId == operation.ProcessingTypeId 
                    && b.Status == "ACTIVE")
                .FirstOrDefaultAsync(cancellationToken);

            if (activeBOM == null)
            {
                _logger.LogWarning("No ACTIVE BOM found for Part {PartCode} + ProcessingType {ProcessingType}. Skipping material check.",
                    operation.Part.Code, operation.ProcessingType.Name);
                continue;
            }

            // Calculate required materials
            foreach (var bomDetail in activeBOM.BOMDetails)
            {
                // Required_Qty = Planned_Qty × BOM_Qty × (1 + Scrap_Rate)
                var requiredQty = request.PlannedQuantity * bomDetail.QuantityPerUnit * (1 + bomDetail.ScrapRate);

                if (!materialRequirements.ContainsKey(bomDetail.MaterialCode))
                {
                    materialRequirements[bomDetail.MaterialCode] = new MaterialRequirement
                    {
                        MaterialCode = bomDetail.MaterialCode,
                        MaterialName = bomDetail.MaterialName,
                        Unit = bomDetail.Unit,
                        TotalRequiredQty = 0
                    };
                }

                materialRequirements[bomDetail.MaterialCode].TotalRequiredQty += requiredQty;
            }
        }

        if (!materialRequirements.Any())
        {
            _logger.LogWarning("No BOM found for PO {PONumber}. Cannot perform availability check.", po.PONumber);
            result.OverallStatus = "WARNING";
            return result;
        }

        // Step 2: Check availability for each material
        foreach (var (materialCode, requirement) in materialRequirements)
        {
            // Get inventory on-hand quantity
            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Code == materialCode, cancellationToken);

            var inventoryQty = material?.CurrentStock ?? 0;

            // Get PO Material Baseline quantity
            var poBaselineQty = po.MaterialBaselines
                .Where(mb => mb.MaterialCode == materialCode)
                .Sum(mb => mb.CommittedQuantity);

            // Available_Qty = Inventory_Qty + PO_Material_Baseline_Qty
            var availableQty = inventoryQty + poBaselineQty;

            // Shortage = Required_Qty - Available_Qty
            var shortage = requirement.TotalRequiredQty - availableQty;

            // Determine severity
            string severity;
            if (shortage > 0)
            {
                severity = "CRITICAL";
                result.OverallStatus = "FAIL";
            }
            else if (availableQty < requirement.TotalRequiredQty * 1.1m)
            {
                severity = "WARNING";
                if (result.OverallStatus == "PASS")
                {
                    result.OverallStatus = "WARNING";
                }
            }
            else
            {
                severity = "OK";
            }

            var detail = new MaterialAvailabilityDetail
            {
                MaterialCode = materialCode,
                MaterialName = requirement.MaterialName,
                RequiredQuantity = requirement.TotalRequiredQty,
                AvailableQuantity = availableQty,
                Shortage = Math.Max(0, shortage),
                Unit = requirement.Unit,
                Severity = severity,
                InventoryQuantity = inventoryQty,
                POBaselineQuantity = poBaselineQty
            };

            result.MaterialDetails.Add(detail);

            _logger.LogInformation("Material {MaterialCode}: Required={Required}, Available={Available}, Shortage={Shortage}, Severity={Severity}",
                materialCode, requirement.TotalRequiredQty, availableQty, shortage, severity);
        }

        _logger.LogInformation("Availability check for PO {PONumber}: Overall status = {Status}, Materials checked = {Count}",
            po.PONumber, result.OverallStatus, result.MaterialDetails.Count);

        return result;
    }
}

/// <summary>
/// Internal class for tracking material requirements
/// </summary>
internal class MaterialRequirement
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal TotalRequiredQty { get; set; }
}



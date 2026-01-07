using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.PurchaseOrders;

/// <summary>
/// Clone PO to create new version
/// PHASE 1: Cloning creates new version (V1, V2, V3...)
/// - Same data
/// - Status = DRAFT
/// - Can be edited until APPROVED_FOR_PMC
/// </summary>
public class ClonePOVersionCommand : IRequest<PurchaseOrderDto>
{
    public Guid OriginalPOId { get; set; }
    public string? Notes { get; set; }
}

public class ClonePOVersionCommandHandler : IRequestHandler<ClonePOVersionCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClonePOVersionCommandHandler> _logger;

    public ClonePOVersionCommandHandler(
        ApplicationDbContext context,
        ILogger<ClonePOVersionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(ClonePOVersionCommand request, CancellationToken cancellationToken)
    {
        // Get original PO with all related data
        var originalPO = await _context.PurchaseOrders
            .Include(p => p.Customer)
            .Include(p => p.POProducts)
            .Include(p => p.POOperations)
            .Include(p => p.MaterialBaselines)
            .FirstOrDefaultAsync(p => p.Id == request.OriginalPOId, cancellationToken);

        if (originalPO == null)
        {
            throw new Exception($"Purchase Order with ID {request.OriginalPOId} not found");
        }

        // PHASE 1: Find root PO (for determining version number)
        var rootPOId = originalPO.OriginalPOId ?? originalPO.Id;
        
        // Get all versions of this PO
        var allVersions = await _context.PurchaseOrders
            .Where(p => p.Id == rootPOId || p.OriginalPOId == rootPOId)
            .ToListAsync(cancellationToken);

        // Determine next version number
        var maxVersionNumber = allVersions.Max(p => p.VersionNumber);
        var nextVersionNumber = maxVersionNumber + 1;
        var nextVersionLabel = $"V{nextVersionNumber}";

        // Create new PO version (PHASE 1: Same data, Status = DRAFT)
        var newPO = new PurchaseOrder
        {
            PONumber = originalPO.PONumber, // Keep same PO Number
            CustomerId = originalPO.CustomerId,
            Version = nextVersionLabel,
            VersionNumber = nextVersionNumber,
            ProcessingType = originalPO.ProcessingType,
            PODate = originalPO.PODate,
            ExpectedDeliveryDate = originalPO.ExpectedDeliveryDate,
            Status = "DRAFT", // PHASE 1: New version starts as DRAFT
            TotalAmount = originalPO.TotalAmount,
            Notes = request.Notes ?? originalPO.Notes,
            OriginalPOId = rootPOId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PurchaseOrders.Add(newPO);

        // Clone POProducts
        foreach (var originalProduct in originalPO.POProducts)
        {
            var newProduct = new POProduct
            {
                PurchaseOrderId = newPO.Id,
                ProductId = originalProduct.ProductId,
                Quantity = originalProduct.Quantity,
                UnitPrice = originalProduct.UnitPrice,
                TotalAmount = originalProduct.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };
            _context.POProducts.Add(newProduct);
        }

        // Clone POOperations
        foreach (var originalOperation in originalPO.POOperations)
        {
            var newOperation = new POOperation
            {
                PurchaseOrderId = newPO.Id,
                PartId = originalOperation.PartId,
                ProcessingTypeId = originalOperation.ProcessingTypeId,
                ProcessMethodId = originalOperation.ProcessMethodId,
                OperationName = originalOperation.OperationName,
                ChargeCount = originalOperation.ChargeCount,
                UnitPrice = originalOperation.UnitPrice,
                Quantity = originalOperation.Quantity,
                TotalAmount = originalOperation.TotalAmount,
                SprayPosition = originalOperation.SprayPosition,
                PrintContent = originalOperation.PrintContent,
                CycleTime = originalOperation.CycleTime,
                AssemblyContent = originalOperation.AssemblyContent,
                SequenceOrder = originalOperation.SequenceOrder,
                Notes = originalOperation.Notes,
                // ÉP NHỰA specific fields
                ModelNumber = originalOperation.ModelNumber,
                Material = originalOperation.Material,
                ColorCode = originalOperation.ColorCode,
                Color = originalOperation.Color,
                CavityQuantity = originalOperation.CavityQuantity,
                Set = originalOperation.Set,
                NetWeight = originalOperation.NetWeight,
                TotalWeight = originalOperation.TotalWeight,
                MachineType = originalOperation.MachineType,
                RequiredMaterial = originalOperation.RequiredMaterial,
                RequiredColor = originalOperation.RequiredColor,
                CompletionDate = originalOperation.CompletionDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.POOperations.Add(newOperation);
        }

        // PHASE 1: Clone Material Baselines
        foreach (var originalBaseline in originalPO.MaterialBaselines)
        {
            var newBaseline = new POMaterialBaseline
            {
                PurchaseOrderId = newPO.Id,
                MaterialCode = originalBaseline.MaterialCode,
                MaterialName = originalBaseline.MaterialName,
                CommittedQuantity = originalBaseline.CommittedQuantity,
                Unit = originalBaseline.Unit,
                ProductCode = originalBaseline.ProductCode,
                PartCode = originalBaseline.PartCode,
                Notes = originalBaseline.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.POMaterialBaselines.Add(newBaseline);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned PO: {OriginalPONumber} {OriginalVersion} to {NewVersion} with ID: {NewPOId}", 
            originalPO.PONumber, originalPO.Version, nextVersionLabel, newPO.Id);

        return new PurchaseOrderDto
        {
            Id = newPO.Id,
            PONumber = newPO.PONumber,
            CustomerId = newPO.CustomerId,
            CustomerName = originalPO.Customer.Name,
            Version = newPO.Version,
            ProcessingType = newPO.ProcessingType,
            PODate = newPO.PODate,
            ExpectedDeliveryDate = newPO.ExpectedDeliveryDate,
            Status = newPO.Status,
            TotalAmount = newPO.TotalAmount,
            Notes = newPO.Notes,
            OriginalPOId = newPO.OriginalPOId,
            VersionNumber = newPO.VersionNumber,
            IsActive = newPO.IsActive,
            CreatedAt = newPO.CreatedAt
        };
    }
}





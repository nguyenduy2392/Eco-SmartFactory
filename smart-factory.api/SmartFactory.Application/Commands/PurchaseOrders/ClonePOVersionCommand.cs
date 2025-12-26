using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.PurchaseOrders;

/// <summary>
/// Clone PO từ version này sang version khác
/// ORIGINAL (PM import) -> FINAL (PM chốt) -> PRODUCTION (PMC điều chỉnh)
/// </summary>
public class ClonePOVersionCommand : IRequest<PurchaseOrderDto>
{
    public Guid OriginalPOId { get; set; }
    public string NewVersionType { get; set; } = "FINAL";
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
            .FirstOrDefaultAsync(p => p.Id == request.OriginalPOId, cancellationToken);

        if (originalPO == null)
        {
            throw new Exception($"Purchase Order with ID {request.OriginalPOId} not found");
        }

        // Validate version flow: ORIGINAL -> FINAL -> PRODUCTION
        if (originalPO.VersionType == "ORIGINAL" && request.NewVersionType != "FINAL")
        {
            throw new Exception("Can only clone ORIGINAL to FINAL version");
        }
        if (originalPO.VersionType == "FINAL" && request.NewVersionType != "PRODUCTION")
        {
            throw new Exception("Can only clone FINAL to PRODUCTION version");
        }
        if (originalPO.VersionType == "PRODUCTION")
        {
            throw new Exception("Cannot clone PRODUCTION version further");
        }

        // Create new PO version
        var newPO = new PurchaseOrder
        {
            PONumber = $"{originalPO.PONumber}-{request.NewVersionType}",
            CustomerId = originalPO.CustomerId,
            VersionType = request.NewVersionType,
            TemplateType = originalPO.TemplateType,
            PODate = originalPO.PODate,
            ExpectedDeliveryDate = originalPO.ExpectedDeliveryDate,
            Status = "New",
            TotalAmount = originalPO.TotalAmount,
            Notes = request.Notes ?? originalPO.Notes,
            OriginalPOId = originalPO.Id,
            VersionNumber = originalPO.VersionNumber + 1,
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
                CreatedAt = DateTime.UtcNow
            };
            _context.POOperations.Add(newOperation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned PO: {OriginalPONumber} to {NewVersionType} with ID: {NewPOId}", 
            originalPO.PONumber, request.NewVersionType, newPO.Id);

        return new PurchaseOrderDto
        {
            Id = newPO.Id,
            PONumber = newPO.PONumber,
            CustomerId = newPO.CustomerId,
            CustomerName = originalPO.Customer.Name,
            VersionType = newPO.VersionType,
            TemplateType = newPO.TemplateType,
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




using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Queries.PurchaseOrders;

public class GetPurchaseOrderByIdQuery : IRequest<PurchaseOrderDto?>
{
    public Guid Id { get; set; }
}

public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto?>
{
    private readonly ApplicationDbContext _context;

    public GetPurchaseOrderByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrderDto?> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Customer)
            .Include(p => p.POProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.POOperations)
                .ThenInclude(po => po.Part)
                    .ThenInclude(part => part.Product)
            .Include(p => p.POOperations)
                .ThenInclude(po => po.Product)
            .Include(p => p.POOperations)
                .ThenInclude(po => po.ProcessingType)
            .Include(p => p.POOperations)
                .ThenInclude(po => po.ProcessMethod)
            .Include(p => p.PurchaseOrderMaterials) // Load materials từ sheet NHAP_NGUYEN_VAT_LIEU
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
        {
            return null;
        }

        // Nếu là Operation PO, lấy PurchaseOrderMaterials từ Original PO
        List<PurchaseOrderMaterial> materials;
        if (po.OriginalPOId.HasValue)
        {
            materials = await _context.PurchaseOrderMaterials
                .Where(m => m.PurchaseOrderId == po.OriginalPOId.Value)
                .ToListAsync(cancellationToken);
        }
        else
        {
            materials = po.PurchaseOrderMaterials.ToList();
        }

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            CustomerId = po.CustomerId,
            CustomerName = po.Customer.Name,
            Version = po.Version,
            ProcessingType = po.ProcessingType,
            PODate = po.PODate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            OriginalPOId = po.OriginalPOId,
            VersionNumber = po.VersionNumber,
            IsActive = po.IsActive,
            CreatedAt = po.CreatedAt,
            CreatedBy = po.CreatedBy,
            Products = po.POProducts.Select(pp => new POProductDto
            {
                Id = pp.Id,
                PurchaseOrderId = pp.PurchaseOrderId,
                ProductId = pp.ProductId,
                ProductCode = pp.Product.Code,
                ProductName = pp.Product.Name,
                Quantity = pp.Quantity,
                UnitPrice = pp.UnitPrice,
                TotalAmount = pp.TotalAmount
            }).ToList(),
            Operations = po.POOperations.Select(op => new POOperationDto
            {
                Id = op.Id,
                PurchaseOrderId = op.PurchaseOrderId,
                PartId = op.PartId ?? Guid.Empty,
                PartCode = op.Part?.Code ?? string.Empty,
                PartName = op.Part?.Name ?? string.Empty,
                ProductId = op.ProductId ?? op.Part?.ProductId,
                ProductCode = op.Product?.Code ?? op.Part?.Product?.Code ?? string.Empty,
                ProductName = op.Product?.Name ?? op.Part?.Product?.Name,
                ProcessingTypeId = op.ProcessingTypeId,
                ProcessingTypeName = op.ProcessingType.Name,
                ProcessMethodId = op.ProcessMethodId,
                ProcessMethodName = op.ProcessMethod?.Name,
                OperationName = op.OperationName,
                ChargeCount = op.ChargeCount,
                UnitPrice = op.UnitPrice,
                ContractUnitPrice = op.ContractUnitPrice,
                Quantity = op.Quantity,
                TotalAmount = op.TotalAmount,
                SprayPosition = op.SprayPosition,
                PrintContent = op.PrintContent,
                CycleTime = op.CycleTime,
                AssemblyContent = op.AssemblyContent,
                // ÉP NHỰA specific fields
                ModelNumber = op.ModelNumber,
                Material = op.Material,
                ColorCode = op.ColorCode,
                Color = op.Color,
                CavityQuantity = op.CavityQuantity,
                Set = op.Set,
                NetWeight = op.NetWeight,
                TotalWeight = op.TotalWeight,
                MachineType = op.MachineType,
                RequiredMaterial = op.RequiredMaterial,
                RequiredColor = op.RequiredColor,
                NumberOfPresses = op.NumberOfPresses,
                Notes = op.Notes,
                CompletionDate = op.CompletionDate,
                SequenceOrder = op.SequenceOrder
            }).OrderBy(op => op.SequenceOrder).ToList(),
            PurchaseOrderMaterials = materials.Select(m => new PurchaseOrderMaterialDto
            {
                Id = m.Id,
                PurchaseOrderId = m.PurchaseOrderId,
                MaterialCode = m.MaterialCode,
                MaterialName = m.MaterialName,
                MaterialType = m.MaterialType,
                PlannedQuantity = m.PlannedQuantity,
                Unit = m.Unit,
                ColorCode = m.ColorCode,
                Notes = m.Notes
            }).ToList()
        };
    }
}




using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

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
            .Include(p => p.POOperations)
                .ThenInclude(po => po.ProcessingType)
            .Include(p => p.POOperations)
                .ThenInclude(po => po.ProcessMethod)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
        {
            return null;
        }

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            CustomerId = po.CustomerId,
            CustomerName = po.Customer.Name,
            VersionType = po.VersionType,
            TemplateType = po.TemplateType,
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
                PartId = op.PartId,
                PartCode = op.Part.Code,
                PartName = op.Part.Name,
                ProcessingTypeId = op.ProcessingTypeId,
                ProcessingTypeName = op.ProcessingType.Name,
                ProcessMethodId = op.ProcessMethodId,
                ProcessMethodName = op.ProcessMethod?.Name,
                OperationName = op.OperationName,
                ChargeCount = op.ChargeCount,
                UnitPrice = op.UnitPrice,
                Quantity = op.Quantity,
                TotalAmount = op.TotalAmount,
                SprayPosition = op.SprayPosition,
                PrintContent = op.PrintContent,
                CycleTime = op.CycleTime,
                AssemblyContent = op.AssemblyContent,
                Notes = op.Notes,
                SequenceOrder = op.SequenceOrder
            }).OrderBy(op => op.SequenceOrder).ToList()
        };
    }
}




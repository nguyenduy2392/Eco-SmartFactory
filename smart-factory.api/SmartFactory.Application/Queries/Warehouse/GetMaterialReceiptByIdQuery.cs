using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

public class GetMaterialReceiptByIdQuery : IRequest<MaterialReceiptDto?>
{
    public Guid Id { get; set; }
}

public class GetMaterialReceiptByIdQueryHandler : IRequestHandler<GetMaterialReceiptByIdQuery, MaterialReceiptDto?>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialReceiptByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialReceiptDto?> Handle(GetMaterialReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _context.MaterialReceipts
            .Include(mr => mr.Customer)
            .Include(mr => mr.Material)
            .Include(mr => mr.Warehouse)
            .FirstOrDefaultAsync(mr => mr.Id == request.Id, cancellationToken);

        if (receipt == null)
        {
            return null;
        }

        return new MaterialReceiptDto
        {
            Id = receipt.Id,
            CustomerId = receipt.CustomerId,
            CustomerName = receipt.Customer.Name,
            MaterialId = receipt.MaterialId,
            MaterialCode = receipt.Material.Code,
            MaterialName = receipt.Material.Name,
            WarehouseId = receipt.WarehouseId,
            WarehouseCode = receipt.Warehouse.Code,
            WarehouseName = receipt.Warehouse.Name,
            Quantity = receipt.Quantity,
            Unit = receipt.Unit,
            BatchNumber = receipt.BatchNumber,
            ReceiptDate = receipt.ReceiptDate,
            SupplierCode = receipt.SupplierCode,
            PurchasePOCode = receipt.PurchasePOCode,
            ReceiptNumber = receipt.ReceiptNumber,
            Notes = receipt.Notes,
            Status = receipt.Status,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt,
            CreatedBy = receipt.CreatedBy,
            UpdatedBy = receipt.UpdatedBy
        };
    }
}


using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

public class GetMaterialReceiptsQuery : IRequest<List<MaterialReceiptDto>>
{
    public Guid? CustomerId { get; set; }
    public Guid? MaterialId { get; set; }
    public string? Status { get; set; }
}

public class GetMaterialReceiptsQueryHandler : IRequestHandler<GetMaterialReceiptsQuery, List<MaterialReceiptDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialReceiptsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialReceiptDto>> Handle(GetMaterialReceiptsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaterialReceipts
            .Include(mr => mr.Customer)
            .Include(mr => mr.Material)
            .Include(mr => mr.Warehouse)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(mr => mr.CustomerId == request.CustomerId.Value);
        }

        if (request.MaterialId.HasValue)
        {
            query = query.Where(mr => mr.MaterialId == request.MaterialId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(mr => mr.Status == request.Status);
        }

        var receipts = await query
            .OrderByDescending(mr => mr.ReceiptDate)
            .ThenByDescending(mr => mr.CreatedAt)
            .ToListAsync(cancellationToken);

        return receipts.Select(mr => new MaterialReceiptDto
        {
            Id = mr.Id,
            CustomerId = mr.CustomerId,
            CustomerName = mr.Customer.Name,
            MaterialId = mr.MaterialId,
            MaterialCode = mr.Material.Code,
            MaterialName = mr.Material.Name,
            WarehouseId = mr.WarehouseId,
            WarehouseCode = mr.Warehouse.Code,
            WarehouseName = mr.Warehouse.Name,
            Quantity = mr.Quantity,
            Unit = mr.Unit,
            BatchNumber = mr.BatchNumber,
            ReceiptDate = mr.ReceiptDate,
            SupplierCode = mr.SupplierCode,
            PurchasePOCode = mr.PurchasePOCode,
            ReceiptNumber = mr.ReceiptNumber,
            Notes = mr.Notes,
            Status = mr.Status,
            CreatedAt = mr.CreatedAt,
            UpdatedAt = mr.UpdatedAt,
            CreatedBy = mr.CreatedBy,
            UpdatedBy = mr.UpdatedBy
        }).ToList();
    }
}


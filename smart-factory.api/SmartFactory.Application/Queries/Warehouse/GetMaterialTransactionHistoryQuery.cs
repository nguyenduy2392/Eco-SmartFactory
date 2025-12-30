using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

/// <summary>
/// Query để lấy lịch sử giao dịch kho của nguyên vật liệu
/// </summary>
public class GetMaterialTransactionHistoryQuery : IRequest<List<MaterialTransactionHistoryDto>>
{
    public Guid? MaterialId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? BatchNumber { get; set; }
    public string? TransactionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}

public class GetMaterialTransactionHistoryQueryHandler : IRequestHandler<GetMaterialTransactionHistoryQuery, List<MaterialTransactionHistoryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialTransactionHistoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialTransactionHistoryDto>> Handle(GetMaterialTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaterialTransactionHistories
            .Include(h => h.Customer)
            .Include(h => h.Material)
            .Include(h => h.Warehouse)
            .AsQueryable();

        if (request.MaterialId.HasValue)
        {
            query = query.Where(h => h.MaterialId == request.MaterialId.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(h => h.CustomerId == request.CustomerId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(h => h.WarehouseId == request.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            query = query.Where(h => h.BatchNumber == request.BatchNumber);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionType))
        {
            query = query.Where(h => h.TransactionType == request.TransactionType);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(h => h.TransactionDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(h => h.TransactionDate <= request.ToDate.Value);
        }

        query = query.OrderByDescending(h => h.TransactionDate);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query.Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                         .Take(request.PageSize.Value);
        }

        var histories = await query.ToListAsync(cancellationToken);

        return histories.Select(h => new MaterialTransactionHistoryDto
        {
            Id = h.Id,
            CustomerId = h.CustomerId,
            CustomerName = h.Customer.Name,
            MaterialId = h.MaterialId,
            MaterialCode = h.Material.Code,
            MaterialName = h.Material.Name,
            WarehouseId = h.WarehouseId,
            WarehouseCode = h.Warehouse.Code,
            WarehouseName = h.Warehouse.Name,
            BatchNumber = h.BatchNumber,
            TransactionType = h.TransactionType,
            ReferenceId = h.ReferenceId,
            ReferenceNumber = h.ReferenceNumber,
            StockBefore = h.StockBefore,
            QuantityChange = h.QuantityChange,
            StockAfter = h.StockAfter,
            Unit = h.Unit,
            TransactionDate = h.TransactionDate,
            CreatedBy = h.CreatedBy,
            Notes = h.Notes,
            CreatedAt = h.CreatedAt
        }).ToList();
    }
}


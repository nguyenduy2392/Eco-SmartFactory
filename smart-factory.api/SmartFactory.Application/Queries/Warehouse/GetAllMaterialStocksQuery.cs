using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

/// <summary>
/// Query để lấy danh sách tồn kho của tất cả nguyên vật liệu (grouped by material + customer)
/// </summary>
public class GetAllMaterialStocksQuery : IRequest<List<MaterialStockDto>>
{
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
}

public class GetAllMaterialStocksQueryHandler : IRequestHandler<GetAllMaterialStocksQuery, List<MaterialStockDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllMaterialStocksQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialStockDto>> Handle(GetAllMaterialStocksQuery request, CancellationToken cancellationToken)
    {
        // Get all transaction histories with related data
        var transactionQuery = _context.MaterialTransactionHistories
            .Include(h => h.Material)
            .Include(h => h.Customer)
            .Include(h => h.Warehouse)
            .Where(h => h.Material.IsActive == true)
            .AsQueryable();

        // Filter by customer if specified
        if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
        {
            transactionQuery = transactionQuery.Where(h => h.CustomerId == request.CustomerId.Value);
        }

        // Filter by warehouse if specified
        if (request.WarehouseId.HasValue)
        {
            transactionQuery = transactionQuery.Where(h => h.WarehouseId == request.WarehouseId.Value);
        }

        var transactions = await transactionQuery.ToListAsync(cancellationToken);

        // Group by (MaterialId + CustomerId) in memory
        var groupedStocks = transactions
            .GroupBy(h => new { h.MaterialId, h.CustomerId })
            .Select(g => new
            {
                MaterialId = g.Key.MaterialId,
                CustomerId = g.Key.CustomerId,
                Material = g.First().Material,
                Customer = g.First().Customer,
                TotalStock = g.Sum(h => h.QuantityChange),
                Unit = g.First().Unit,
                Transactions = g.ToList()
            })
            .Where(g => g.TotalStock > 0) // Only show materials with positive stock
            .OrderBy(g => g.Material.Code)
            .ThenBy(g => g.Customer?.Name)
            .ToList();

        var result = groupedStocks.Select(stock => {
            var batchStocks = stock.Transactions
                .GroupBy(h => h.BatchNumber)
                .Select(g => new BatchStockDto
                {
                    BatchNumber = g.Key,
                    Quantity = g.Sum(h => h.QuantityChange),
                    Unit = g.First().Unit,
                    LastReceiptDate = g
                        .Where(h => h.TransactionType == "RECEIPT")
                        .Max(h => (DateTime?)h.TransactionDate),
                    LastIssueDate = g
                        .Where(h => h.TransactionType == "ISSUE")
                        .Max(h => (DateTime?)h.TransactionDate)
                })
                .Where(b => b.Quantity > 0)
                .ToList();

            Entities.Warehouse? warehouse = null;
            if (request.WarehouseId.HasValue)
            {
                warehouse = stock.Transactions.First().Warehouse;
            }

            return new MaterialStockDto
            {
                MaterialId = stock.MaterialId,
                MaterialCode = stock.Material.Code,
                MaterialName = stock.Material.Name,
                CustomerId = stock.CustomerId,
                CustomerName = stock.Customer?.Name,
                WarehouseId = warehouse?.Id,
                WarehouseCode = warehouse?.Code,
                WarehouseName = warehouse?.Name,
                CurrentStock = stock.TotalStock,
                MinStock = stock.Material.MinStock,
                Unit = stock.Unit,
                BatchStocks = batchStocks
            };
        }).ToList();
        return result;
    }
}

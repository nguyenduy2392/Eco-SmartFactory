using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

/// <summary>
/// Query để lấy thông tin tồn kho của nguyên vật liệu
/// </summary>
public class GetMaterialStockQuery : IRequest<MaterialStockDto>
{
    public Guid MaterialId { get; set; }
    public Guid? WarehouseId { get; set; }
}

public class MaterialStockDto
{
    public Guid MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public string Unit { get; set; } = string.Empty;
    public List<BatchStockDto> BatchStocks { get; set; } = new();
}

public class BatchStockDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? LastReceiptDate { get; set; }
    public DateTime? LastIssueDate { get; set; }
}

public class GetMaterialStockQueryHandler : IRequestHandler<GetMaterialStockQuery, MaterialStockDto>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialStockQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialStockDto> Handle(GetMaterialStockQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId, cancellationToken);

        if (material == null)
        {
            throw new Exception($"Material with ID {request.MaterialId} not found");
        }

        Entities.Warehouse? warehouse = null;
        if (request.WarehouseId.HasValue)
        {
            warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId.Value, cancellationToken);
        }

        // Calculate batch stocks
        var batchQuery = _context.MaterialTransactionHistories
            .Where(h => h.MaterialId == request.MaterialId);

        if (request.WarehouseId.HasValue)
        {
            batchQuery = batchQuery.Where(h => h.WarehouseId == request.WarehouseId.Value);
        }

        var batchTransactions = await batchQuery
            .OrderBy(h => h.BatchNumber)
            .ThenBy(h => h.TransactionDate)
            .ToListAsync(cancellationToken);

        var batchStocks = batchTransactions
            .GroupBy(h => h.BatchNumber)
            .Select(g => new BatchStockDto
            {
                BatchNumber = g.Key,
                Quantity = g.Sum(h => h.QuantityChange),
                Unit = material.Unit,
                LastReceiptDate = g.Where(h => h.TransactionType == "RECEIPT").Max(h => (DateTime?)h.TransactionDate),
                LastIssueDate = g.Where(h => h.TransactionType == "ISSUE").Max(h => (DateTime?)h.TransactionDate)
            })
            .Where(b => b.Quantity > 0) // Only show batches with remaining stock
            .ToList();

        return new MaterialStockDto
        {
            MaterialId = material.Id,
            MaterialCode = material.Code,
            MaterialName = material.Name,
            CustomerId = material.CustomerId,
            CustomerName = material.Customer.Name,
            WarehouseId = warehouse?.Id,
            WarehouseCode = warehouse?.Code,
            WarehouseName = warehouse?.Name,
            CurrentStock = material.CurrentStock,
            MinStock = material.MinStock,
            Unit = material.Unit,
            BatchStocks = batchStocks
        };
    }
}


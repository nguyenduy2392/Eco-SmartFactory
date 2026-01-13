using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service quản lý nghiệp vụ nhập kho (Stock In)
/// Hỗ trợ nhập kho có gắn hoặc không gắn PO
/// </summary>
public class StockInService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockInService> _logger;

    public StockInService(ApplicationDbContext context, ILogger<StockInService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Xử lý nhập kho nguyên vật liệu
    /// - Luôn ghi nhận vào bảng kho (MaterialReceipt)
    /// - Nếu có gắn PO: tạo lịch sử nhập kho cho PO (MaterialReceiptHistory)
    /// - Cập nhật tồn kho Material
    /// - Tạo MaterialTransactionHistory
    /// </summary>
    public async Task<StockInResult> ProcessStockInAsync(StockInRequest request, string? currentUser)
    {
        var result = new StockInResult();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate PO nếu có
            PurchaseOrder? purchaseOrder = null;
            if (request.PurchaseOrderId.HasValue)
            {
                purchaseOrder = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId.Value);

                if (purchaseOrder == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "PO không tồn tại";
                    return result;
                }
            }

            // Validate warehouse
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId);

            if (warehouse == null)
            {
                result.Success = false;
                result.ErrorMessage = "Kho không tồn tại";
                return result;
            }

            // Validate customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId);

            if (customer == null)
            {
                result.Success = false;
                result.ErrorMessage = "Khách hàng không tồn tại";
                return result;
            }

            var createdReceipts = new List<MaterialReceipt>();
            var createdHistories = new List<MaterialReceiptHistory>();

            // Xử lý từng nguyên vật liệu
            var itemIndex = 1;
            foreach (var item in request.Materials)
            {
                // Validate material
                var material = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Id == item.MaterialId);

                if (material == null)
                {
                    result.Errors.Add($"Nguyên vật liệu {item.MaterialId} không tồn tại");
                    continue;
                }

                // Generate unique receipt number for each material item
                var uniqueReceiptNumber = request.Materials.Count > 1 
                    ? $"{request.ReceiptNumber}-{itemIndex:D3}"
                    : request.ReceiptNumber;

                // 1. Tạo MaterialReceipt (nhập kho thực tế)
                var receipt = new MaterialReceipt
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    MaterialId = item.MaterialId,
                    WarehouseId = request.WarehouseId,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    BatchNumber = item.BatchNumber,
                    ReceiptDate = request.ReceiptDate,
                    SupplierCode = item.SupplierCode,
                    PurchasePOCode = item.PurchasePOCode,
                    ReceiptNumber = uniqueReceiptNumber,
                    Notes = item.Notes ?? request.Notes,
                    Status = "RECEIVED",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser
                };
                
                itemIndex++;

                _context.MaterialReceipts.Add(receipt);
                createdReceipts.Add(receipt);

                // 2. Nếu có gắn PO: tạo MaterialReceiptHistory
                if (request.PurchaseOrderId.HasValue)
                {
                    var history = new MaterialReceiptHistory
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = request.PurchaseOrderId.Value,
                        MaterialReceiptId = receipt.Id,
                        MaterialId = item.MaterialId,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        BatchNumber = item.BatchNumber,
                        ReceiptDate = request.ReceiptDate,
                        CreatedBy = currentUser,
                        Notes = item.Notes ?? request.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MaterialReceiptHistories.Add(history);
                    createdHistories.Add(history);
                }

                // 3. Cập nhật tồn kho Material
                var stockBefore = material.CurrentStock;
                material.CurrentStock += item.Quantity;
                material.UpdatedAt = DateTime.UtcNow;

                // 4. Tạo MaterialTransactionHistory
                var transactionHistory = new MaterialTransactionHistory
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    MaterialId = item.MaterialId,
                    WarehouseId = request.WarehouseId,
                    BatchNumber = item.BatchNumber,
                    TransactionType = "RECEIPT",
                    TransactionDate = request.ReceiptDate,
                    ReferenceId = receipt.Id,
                    ReferenceNumber = request.ReceiptNumber,
                    StockBefore = stockBefore,
                    QuantityChange = item.Quantity,
                    StockAfter = material.CurrentStock,
                    Unit = item.Unit,
                    Notes = $"Nhập kho - {request.Notes ?? ""}",
                    CreatedBy = currentUser,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MaterialTransactionHistories.Add(transactionHistory);
            }

            // Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.CreatedReceiptIds = createdReceipts.Select(r => r.Id).ToList();
            result.CreatedHistoryIds = createdHistories.Select(h => h.Id).ToList();
            result.Message = $"Đã nhập kho thành công {createdReceipts.Count} nguyên vật liệu";

            _logger.LogInformation("Stock in completed successfully. {Count} receipts created", createdReceipts.Count);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing stock in");
            result.Success = false;
            result.ErrorMessage = $"Lỗi khi nhập kho: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Lấy lịch sử nhập kho của một PO
    /// Nếu là Operation PO (có OriginalPOId), lấy history của Original PO
    /// Nếu là Original PO, lấy history của chính nó
    /// </summary>
    public async Task<List<MaterialReceiptHistoryDto>> GetPOReceiptHistoryAsync(Guid purchaseOrderId)
    {
        // Tìm PO và xác định có phải Operation PO không
        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

        if (po == null)
        {
            return new List<MaterialReceiptHistoryDto>();
        }

        // Nếu là Operation PO, lấy history của Original PO
        // Nếu là Original PO, lấy history của chính nó
        var targetPOId = po.OriginalPOId ?? purchaseOrderId;

        var histories = await _context.MaterialReceiptHistories
            .Include(h => h.Material)
            .Include(h => h.MaterialReceipt)
            .Include(h => h.PurchaseOrder)
            .Where(h => h.PurchaseOrderId == targetPOId)
            .OrderByDescending(h => h.ReceiptDate)
            .Select(h => new MaterialReceiptHistoryDto
            {
                Id = h.Id,
                PurchaseOrderId = h.PurchaseOrderId,
                PONumber = h.PurchaseOrder != null ? h.PurchaseOrder.PONumber : null,
                MaterialReceiptId = h.MaterialReceiptId,
                ReceiptNumber = h.MaterialReceipt.ReceiptNumber,
                MaterialId = h.MaterialId,
                MaterialCode = h.Material.Code,
                MaterialName = h.Material.Name,
                Quantity = h.Quantity,
                Unit = h.Unit,
                BatchNumber = h.BatchNumber,
                ReceiptDate = h.ReceiptDate,
                CreatedBy = h.CreatedBy,
                Notes = h.Notes
            })
            .ToListAsync();

        return histories;
    }

    /// <summary>
    /// Lấy danh sách PO để chọn khi nhập kho
    /// </summary>
    public async Task<List<POForSelectionDto>> GetPOsForSelectionAsync(string? searchTerm = null, Guid? customerId = null)
    {
        var query = _context.PurchaseOrders
            .Where(po => po.IsActive)
            .AsQueryable();

        // Filter theo customer nếu có
        if (customerId.HasValue)
        {
            query = query.Where(po => po.CustomerId == customerId.Value);
        }

        // Filter theo search term nếu có
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(po => po.PONumber.Contains(searchTerm));
        }

        var pos = await query
            .Include(po => po.Customer)
            .OrderByDescending(po => po.PODate)
            .Take(50)
            .Select(po => new POForSelectionDto
            {
                Id = po.Id,
                PONumber = po.PONumber,
                CustomerName = po.Customer.Name,
                PODate = po.PODate,
                Status = po.Status,
                IsMaterialFullyReceived = po.IsMaterialFullyReceived
            })
            .ToListAsync();

        return pos;
    }
}

public class StockInResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Guid> CreatedReceiptIds { get; set; } = new();
    public List<Guid> CreatedHistoryIds { get; set; } = new();
}

public class POForSelectionDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime PODate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsMaterialFullyReceived { get; set; }
}

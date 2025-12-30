namespace SmartFactory.Application.Entities;

/// <summary>
/// Lịch sử giao dịch kho - CỰC KỲ QUAN TRỌNG
/// Mỗi biến động kho bắt buộc sinh lịch sử
/// Không được xóa lịch sử
/// </summary>
public class MaterialTransactionHistory
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Chủ hàng sở hữu nguyên vật liệu
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Nguyên vật liệu
    /// </summary>
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Kho
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Số lô (Batch Number)
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại giao dịch: RECEIPT (Nhập), ISSUE (Xuất), ADJUSTMENT (Điều chỉnh)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID của phiếu gốc (MaterialReceiptId, MaterialIssueId, hoặc MaterialAdjustmentId)
    /// </summary>
    public Guid? ReferenceId { get; set; }
    
    /// <summary>
    /// Số phiếu gốc
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Tồn trước khi thay đổi
    /// </summary>
    public decimal StockBefore { get; set; }
    
    /// <summary>
    /// Số lượng thay đổi (+ cho nhập, - cho xuất, +/- cho điều chỉnh)
    /// </summary>
    public decimal QuantityChange { get; set; }
    
    /// <summary>
    /// Tồn sau khi thay đổi
    /// </summary>
    public decimal StockAfter { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian giao dịch
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Người thao tác
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Lý do / Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}


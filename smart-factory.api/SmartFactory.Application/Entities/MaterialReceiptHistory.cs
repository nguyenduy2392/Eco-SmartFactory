namespace SmartFactory.Application.Entities;

/// <summary>
/// Lịch sử nhập kho cho PO
/// Lưu thông tin các lần nhập kho liên quan đến PO
/// </summary>
public class MaterialReceiptHistory
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// PO liên quan (nullable - có thể nhập kho không gắn PO)
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Material Receipt (phiếu nhập kho thực tế)
    /// </summary>
    public Guid MaterialReceiptId { get; set; }
    
    /// <summary>
    /// Nguyên vật liệu
    /// </summary>
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Số lượng nhập
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lô
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ngày giờ nhập kho
    /// </summary>
    public DateTime ReceiptDate { get; set; }
    
    /// <summary>
    /// Người thao tác
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    public virtual MaterialReceipt MaterialReceipt { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
}

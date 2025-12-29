namespace SmartFactory.Application.Entities;

/// <summary>
/// PO Material Baseline - từ sheet NHAP_NGUYEN_VAT_LIEU trong file Excel PO
/// Represents customer-committed materials for availability check
/// IMPORTANT: 
/// - Đây là CAM KẾT từ chủ hàng, KHÔNG phải nhập kho thực tế
/// - Used ONLY for availability check, does NOT affect pricing or settlement
/// - Nhập kho thực tế được quản lý qua MaterialReceipt entity
/// </summary>
public class POMaterialBaseline
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// PO chứa material baseline này
    /// </summary>
    public Guid PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Mã nguyên vật liệu
    /// </summary>
    public string MaterialCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên nguyên vật liệu
    /// </summary>
    public string MaterialName { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng cam kết từ khách hàng
    /// </summary>
    public decimal CommittedQuantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính (kg, liter, pcs, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Mã sản phẩm liên quan (optional)
    /// </summary>
    public string? ProductCode { get; set; }
    
    /// <summary>
    /// Mã linh kiện liên quan (optional)
    /// </summary>
    public string? PartCode { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}



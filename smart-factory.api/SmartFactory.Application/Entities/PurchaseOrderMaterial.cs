namespace SmartFactory.Application.Entities;

/// <summary>
/// Bảng trung gian lưu danh sách nguyên vật liệu từ sheet NVL trong file Excel PO
/// KHÔNG phải dữ liệu nhập kho thực tế
/// Chỉ dùng để hiển thị danh sách NVL cần cho PO (kế hoạch/định mức)
/// </summary>
public class PurchaseOrderMaterial
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// PO chứa nguyên vật liệu này
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
    /// Loại nguyên vật liệu
    /// </summary>
    public string? MaterialType { get; set; }
    
    /// <summary>
    /// Số lượng kế hoạch
    /// </summary>
    public decimal PlannedQuantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Mã màu (nếu có)
    /// </summary>
    public string? ColorCode { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}

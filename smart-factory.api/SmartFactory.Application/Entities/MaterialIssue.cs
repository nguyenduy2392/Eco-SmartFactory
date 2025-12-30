namespace SmartFactory.Application.Entities;

/// <summary>
/// Phiếu xuất kho nguyên vật liệu - Ghi nhận vật liệu đã được sử dụng
/// Chỉ ghi nhận khi sản xuất thực tế hoặc hao hụt được xác nhận
/// </summary>
public class MaterialIssue
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Chủ hàng sở hữu nguyên vật liệu
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Nguyên vật liệu được xuất
    /// </summary>
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Kho xuất
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Số lô (Batch Number) - BẮT BUỘC
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng xuất
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Ngày xuất kho
    /// </summary>
    public DateTime IssueDate { get; set; }
    
    /// <summary>
    /// Lý do xuất (Sản xuất, Hao hụt, Điều chỉnh...)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Số phiếu xuất
    /// </summary>
    public string IssueNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Trạng thái: PENDING, ISSUED, CANCELLED
    /// </summary>
    public string Status { get; set; } = "PENDING";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}


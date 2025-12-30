namespace SmartFactory.Application.Entities;

/// <summary>
/// Phiếu điều chỉnh kho - Sửa chênh lệch tồn kho
/// Không phải nghiệp vụ thường xuyên
/// Bắt buộc có lý do rõ ràng và người chịu trách nhiệm
/// </summary>
public class MaterialAdjustment
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Chủ hàng sở hữu nguyên vật liệu
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Nguyên vật liệu được điều chỉnh
    /// </summary>
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Kho điều chỉnh
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Số lô (Batch Number) - BẮT BUỘC
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng điều chỉnh (có thể âm hoặc dương)
    /// </summary>
    public decimal AdjustmentQuantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Ngày điều chỉnh
    /// </summary>
    public DateTime AdjustmentDate { get; set; }
    
    /// <summary>
    /// Lý do điều chỉnh - BẮT BUỘC
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Người chịu trách nhiệm - BẮT BUỘC
    /// </summary>
    public string ResponsiblePerson { get; set; } = string.Empty;
    
    /// <summary>
    /// Số phiếu điều chỉnh
    /// </summary>
    public string AdjustmentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ghi chú chi tiết
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Trạng thái: PENDING, APPROVED, CANCELLED
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


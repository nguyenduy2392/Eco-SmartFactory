namespace SmartFactory.Application.Entities;

/// <summary>
/// PO Gia công - Đơn đặt hàng gia công do chủ hàng gửi cho Hải Tân
/// Có 3 phiên bản: ORIGINAL (từ Excel), FINAL (PM chốt), PRODUCTION (PMC điều chỉnh)
/// </summary>
public class PurchaseOrder
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã PO (ví dụ: PO-2023-1024)
    /// </summary>
    public string PONumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Chủ hàng
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Loại phiên bản: ORIGINAL, FINAL, PRODUCTION
    /// </summary>
    public string VersionType { get; set; } = "ORIGINAL";
    
    /// <summary>
    /// Loại template import: EP_NHUA, LAP_RAP, PHUN_IN
    /// </summary>
    public string? TemplateType { get; set; }
    
    /// <summary>
    /// Ngày nhận PO
    /// </summary>
    public DateTime PODate { get; set; }
    
    /// <summary>
    /// Ngày giao dự kiến
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    /// <summary>
    /// Trạng thái: New, InProgress, Completed, Cancelled
    /// </summary>
    public string Status { get; set; } = "New";
    
    /// <summary>
    /// Tổng tiền tạm tính
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// PO gốc (nếu đây là bản FINAL hoặc PRODUCTION)
    /// </summary>
    public Guid? OriginalPOId { get; set; }
    
    /// <summary>
    /// Số phiên bản
    /// </summary>
    public int VersionNumber { get; set; } = 1;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual PurchaseOrder? OriginalPO { get; set; }
    public virtual ICollection<PurchaseOrder> DerivedVersions { get; set; } = new List<PurchaseOrder>();
    public virtual ICollection<POProduct> POProducts { get; set; } = new List<POProduct>();
    public virtual ICollection<POOperation> POOperations { get; set; } = new List<POOperation>();
}



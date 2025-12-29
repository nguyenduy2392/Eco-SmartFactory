namespace SmartFactory.Application.Entities;

/// <summary>
/// PO Gia công - Đơn đặt hàng gia công do chủ hàng gửi cho Hải Tân
/// PHASE 1: PO is a FINANCIAL BASELINE defining pricing and settlement only
/// Phiên bản: V0 (original), V1, V2... (immutable once LOCKED)
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
    /// Version label: V0, V1, V2, etc.
    /// V0 is the original imported version
    /// </summary>
    public string Version { get; set; } = "V0";
    
    /// <summary>
    /// Status: DRAFT, APPROVED_FOR_PMC, LOCKED
    /// - DRAFT: Can be edited
    /// - APPROVED_FOR_PMC: Approved for production planning (only ONE version can have this status)
    /// - LOCKED: Immutable, cannot be edited
    /// </summary>
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// Loại template import: EP_NHUA, LAP_RAP, PHUN_IN
    /// </summary>
    public string? ProcessingType { get; set; }
    
    /// <summary>
    /// Ngày nhận PO
    /// </summary>
    public DateTime PODate { get; set; }
    
    /// <summary>
    /// Ngày giao dự kiến
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    /// <summary>
    /// Tổng tiền tạm tính (for financial baseline)
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// PO gốc (nếu đây là version > V0)
    /// </summary>
    public Guid? OriginalPOId { get; set; }
    
    /// <summary>
    /// Số phiên bản (numeric: 0, 1, 2...)
    /// </summary>
    public int VersionNumber { get; set; } = 0;
    
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
    public virtual ICollection<POMaterialBaseline> MaterialBaselines { get; set; } = new List<POMaterialBaseline>();
}





namespace SmartFactory.Application.Entities;

/// <summary>
/// Chủ hàng / Nhà máy - Đơn vị sở hữu sản phẩm, ký hợp đồng gia công với Hải Tân
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã chủ hàng (ví dụ: C-001)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên công ty chủ hàng
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Người liên hệ
    /// </summary>
    public string? ContactPerson { get; set; }
    
    /// <summary>
    /// Email liên hệ
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Điều khoản thanh toán (ví dụ: NET 30, NET 60)
    /// </summary>
    public string? PaymentTerms { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}



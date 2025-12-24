namespace SmartFactory.Application.Entities;

/// <summary>
/// Sản phẩm - Mã sản phẩm tổng (ví dụ: PKW4180-0002)
/// Một PO có thể có nhiều sản phẩm
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã sản phẩm (SKU)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả sản phẩm
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Hình ảnh sản phẩm
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Danh mục sản phẩm
    /// </summary>
    public string? Category { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    public virtual ICollection<POProduct> POProducts { get; set; } = new List<POProduct>();
}


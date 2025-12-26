namespace SmartFactory.Application.Entities;

/// <summary>
/// Linh kiện - Chi tiết cấu thành sản phẩm, là đối tượng gia công thực tế
/// Ví dụ: Thân trên, Thân dưới, Đầu, Chân
/// </summary>
public class Part
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã linh kiện (Part ID)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên linh kiện
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Sản phẩm chứa linh kiện này
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Vị trí (trước/sau/trái/phải...)
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Chất liệu (ABS, PVC, PP...)
    /// </summary>
    public string? Material { get; set; }
    
    /// <summary>
    /// Màu sắc
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Trọng lượng (gram)
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<POOperation> POOperations { get; set; } = new List<POOperation>();
}




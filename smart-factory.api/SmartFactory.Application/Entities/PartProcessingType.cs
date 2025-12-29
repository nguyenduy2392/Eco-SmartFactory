namespace SmartFactory.Application.Entities;

/// <summary>
/// Quan hệ nhiều-nhiều giữa Linh kiện (Part) và Loại hình gia công (ProcessingType)
/// Một linh kiện có thể trải qua nhiều loại hình gia công
/// Một loại hình gia công có thể áp dụng cho nhiều linh kiện
/// </summary>
public class PartProcessingType
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Linh kiện
    /// </summary>
    public Guid PartId { get; set; }
    
    /// <summary>
    /// Loại hình gia công
    /// </summary>
    public Guid ProcessingTypeId { get; set; }
    
    /// <summary>
    /// Thứ tự ưu tiên (nếu một linh kiện có nhiều loại hình gia công)
    /// </summary>
    public int SequenceOrder { get; set; }
    
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
    public virtual Part Part { get; set; } = null!;
    public virtual ProcessingType ProcessingType { get; set; } = null!;
}



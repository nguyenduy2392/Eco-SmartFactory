namespace SmartFactory.Application.Entities;

/// <summary>
/// Loại hình gia công - Nhóm gia công lớn (ÉP, SƠN, LẮP RÁP)
/// </summary>
public class ProcessingType
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã loại hình (EP, SON, LAP_RAP)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên loại hình (ÉP NHỰA, SƠN, LẮP RÁP)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ProcessMethod> ProcessMethods { get; set; } = new List<ProcessMethod>();
}


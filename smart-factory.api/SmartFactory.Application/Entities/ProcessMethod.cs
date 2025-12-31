namespace SmartFactory.Application.Entities;

/// <summary>
/// Loại công đoạn - Phương pháp gia công trong một loại hình
/// Ví dụ (thuộc SƠN): Phun kẹp, Phun tay biên, In sơn, Kẻ vẽ, Xóc màu
/// </summary>
public class ProcessMethod
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã phương pháp (PHUN_KEP, IN_SON, KE_VE...)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên phương pháp
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại hình gia công
    /// </summary>
    public Guid ProcessingTypeId { get; set; }
    
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
    public virtual ProcessingType ProcessingType { get; set; } = null!;
}










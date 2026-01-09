namespace SmartFactory.Application.Entities;

/// <summary>
/// Đơn vị tính - Unit of Measure
/// Quản lý các đơn vị tính được sử dụng trong hệ thống
/// Ví dụ: kg, lít, cái, bộ, mét, m², m³, tấn, gram
/// </summary>
public class UnitOfMeasure
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã đơn vị (kg, l, pcs, set, m, m2, m3, ton, g)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên đơn vị hiển thị (Kilogram, Lít, Cái, Bộ, Mét, Mét vuông, Mét khối, Tấn, Gram)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

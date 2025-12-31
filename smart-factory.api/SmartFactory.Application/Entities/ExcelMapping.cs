namespace SmartFactory.Application.Entities;

/// <summary>
/// Cấu hình Template Excel - Lưu thông tin mapping cột cho 3 loại hình
/// ÉP NHỰA (trọng lượng, chu kỳ), LẮP RÁP (nội dung lắp ráp), PHUN IN (vị trí phun, nội dung in)
/// </summary>
public class ExcelMapping
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Loại template: EP_NHUA, LAP_RAP, PHUN_IN
    /// </summary>
    public string TemplateType { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên template
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên cột trong Excel
    /// </summary>
    public string ExcelColumnName { get; set; } = string.Empty;
    
    /// <summary>
    /// Vị trí cột (A, B, C... hoặc số 1, 2, 3...)
    /// </summary>
    public string? ColumnPosition { get; set; }
    
    /// <summary>
    /// Tên trường trong hệ thống (ProductCode, PartName, UnitPrice...)
    /// </summary>
    public string SystemFieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// Kiểu dữ liệu: String, Number, Decimal, Date
    /// </summary>
    public string DataType { get; set; } = "String";
    
    /// <summary>
    /// Bắt buộc hay không
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Giá trị mặc định nếu rỗng
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Thứ tự
    /// </summary>
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}










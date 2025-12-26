namespace SmartFactory.Application.Entities;

/// <summary>
/// Máy móc thiết bị sản xuất
/// </summary>
public class Machine
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã máy (MAY-EP-01, MAY-SON-02)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên máy
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại máy: InjectionMolding (Ép nhựa), Painting (Sơn), Assembly (Lắp ráp)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Model / Hiệu máy
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Nhà sản xuất
    /// </summary>
    public string? Manufacturer { get; set; }
    
    /// <summary>
    /// Trạng thái: Available, InUse, Maintenance, Broken
    /// </summary>
    public string Status { get; set; } = "Available";
    
    /// <summary>
    /// Vị trí trong xưởng
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Công suất tối đa (units/hour)
    /// </summary>
    public decimal? MaxCapacity { get; set; }
    
    /// <summary>
    /// Năm sản xuất
    /// </summary>
    public int? YearManufactured { get; set; }
    
    /// <summary>
    /// Ngày mua
    /// </summary>
    public DateTime? PurchaseDate { get; set; }
    
    /// <summary>
    /// Ngày bảo trì gần nhất
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ProductionOperation> ProductionOperations { get; set; } = new List<ProductionOperation>();
}




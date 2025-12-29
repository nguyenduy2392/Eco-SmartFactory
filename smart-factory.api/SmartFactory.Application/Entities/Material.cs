namespace SmartFactory.Application.Entities;

/// <summary>
/// Vật tư / Nguyên liệu - Thứ bị tiêu hao trong quá trình gia công
/// Ví dụ: Nhựa ABS, Sơn PMS 340C, Dung môi, Mực in
/// Do chủ hàng cung cấp, Hải Tân quản lý tồn & hao hụt
/// </summary>
public class Material
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã vật tư (MAT-00124)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên vật tư
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại vật tư: Plastic, Paint, Solvent, Ink, Component
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Mã màu (đối với sơn) - ví dụ: PMS 340C
    /// </summary>
    public string? ColorCode { get; set; }
    
    /// <summary>
    /// Chủ hàng sở hữu nguyên vật liệu này
    /// Nguyên vật liệu do chủ hàng cung cấp cho Hải Tân để gia công
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Nhà cung cấp / Chủ hàng cung cấp (tên text - có thể để null nếu dùng CustomerId)
    /// </summary>
    public string? Supplier { get; set; }
    
    /// <summary>
    /// Đơn vị tính (kg, liter, unit...)
    /// </summary>
    public string Unit { get; set; } = "kg";
    
    /// <summary>
    /// Tồn kho hiện tại (tính từ tổng các MaterialReceipt)
    /// </summary>
    public decimal CurrentStock { get; set; }
    
    /// <summary>
    /// Tồn kho tối thiểu
    /// </summary>
    public decimal MinStock { get; set; }
    
    /// <summary>
    /// Đơn giá (để tính chi phí)
    /// </summary>
    public decimal? UnitCost { get; set; }
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<ProductionOperation> ProductionOperations { get; set; } = new List<ProductionOperation>();
    public virtual ICollection<ProductionOperationMaterial> ProductionOperationMaterials { get; set; } = new List<ProductionOperationMaterial>();
    public virtual ICollection<MaterialReceipt> MaterialReceipts { get; set; } = new List<MaterialReceipt>();
}






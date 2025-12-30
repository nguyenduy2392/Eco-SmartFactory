namespace SmartFactory.Application.Entities;

/// <summary>
/// Vật tư sử dụng trong công đoạn thực tế
/// Một công đoạn có thể dùng nhiều vật tư (ví dụ: nhiều màu sơn)
/// </summary>
public class ProductionOperationMaterial
{
    public Guid Id { get; set; }
    
    public Guid ProductionOperationId { get; set; }
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Số lượng vật tư cần dùng (theo đơn vị của vật tư)
    /// </summary>
    public decimal QuantityRequired { get; set; }
    
    /// <summary>
    /// Số lượng đã sử dụng
    /// </summary>
    public decimal QuantityUsed { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual ProductionOperation ProductionOperation { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
}







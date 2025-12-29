namespace SmartFactory.Application.Entities;

/// <summary>
/// Process BOM Detail - Material line in BOM
/// Defines material consumption per 1 PCS
/// </summary>
public class ProcessBOMDetail
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// BOM chứa detail này
    /// </summary>
    public Guid ProcessBOMId { get; set; }
    
    /// <summary>
    /// Mã nguyên vật liệu
    /// </summary>
    public string MaterialCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên nguyên vật liệu
    /// </summary>
    public string MaterialName { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng tiêu hao per 1 PCS
    /// </summary>
    public decimal QuantityPerUnit { get; set; }
    
    /// <summary>
    /// Tỷ lệ hao hụt (>=0, ví dụ: 0.05 = 5%)
    /// </summary>
    public decimal ScrapRate { get; set; } = 0;
    
    /// <summary>
    /// Đơn vị tính (kg, liter, pcs, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Công đoạn sử dụng (for traceability, optional)
    /// </summary>
    public string? ProcessStep { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Thứ tự
    /// </summary>
    public int SequenceOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ProcessBOM ProcessBOM { get; set; } = null!;
}



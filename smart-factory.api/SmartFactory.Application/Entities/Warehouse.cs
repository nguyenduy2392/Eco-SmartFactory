namespace SmartFactory.Application.Entities;

/// <summary>
/// Kho - Nơi lưu trữ nguyên vật liệu
/// Có thể có nhiều kho (RAW, FINISHED, WIP...)
/// </summary>
public class Warehouse
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã kho (ví dụ: RAW, FINISHED, WIP)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên kho
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Địa chỉ kho
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<MaterialReceipt> MaterialReceipts { get; set; } = new List<MaterialReceipt>();
    public virtual ICollection<MaterialIssue> MaterialIssues { get; set; } = new List<MaterialIssue>();
    public virtual ICollection<MaterialAdjustment> MaterialAdjustments { get; set; } = new List<MaterialAdjustment>();
}


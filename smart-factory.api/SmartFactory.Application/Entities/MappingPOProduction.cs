namespace SmartFactory.Application.Entities;

/// <summary>
/// Bảng mapping giữa PO Operation và Production Operation
/// Đây là bảng CHÌA KHÓA để liên kết lớp tính tiền và lớp thực tế
/// </summary>
public class MappingPOProduction
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Công đoạn theo PO (tính tiền)
    /// </summary>
    public Guid POOperationId { get; set; }
    
    /// <summary>
    /// Công đoạn thực tế sản xuất
    /// </summary>
    public Guid ProductionOperationId { get; set; }
    
    /// <summary>
    /// Tỷ lệ quy đổi (để quy đổi sản lượng)
    /// Ví dụ: 1 PO Operation = 0.5 Production Operation (gộp 2 thành 1)
    /// </summary>
    public decimal AllocationRatio { get; set; } = 1.0m;
    
    /// <summary>
    /// Ghi chú về mapping
    /// </summary>
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // Navigation properties
    public virtual POOperation POOperation { get; set; } = null!;
    public virtual ProductionOperation ProductionOperation { get; set; } = null!;
}




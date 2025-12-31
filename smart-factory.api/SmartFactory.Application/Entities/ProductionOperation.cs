namespace SmartFactory.Application.Entities;

/// <summary>
/// Công đoạn thực tế sản xuất (Production Operation)
/// Là cách Hải Tân thực sự triển khai sản xuất, do PMC cấu hình
/// Có thể khác với PO, gắn với máy, tool, nhân sự cụ thể
/// </summary>
public class ProductionOperation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// PO liên quan
    /// </summary>
    public Guid PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Linh kiện được gia công
    /// </summary>
    public Guid PartId { get; set; }
    
    /// <summary>
    /// Tên công đoạn thực tế
    /// </summary>
    public string OperationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Phương pháp gia công thực tế
    /// </summary>
    public Guid? ProcessMethodId { get; set; }
    
    /// <summary>
    /// Máy sử dụng
    /// </summary>
    public Guid? MachineId { get; set; }
    
    /// <summary>
    /// Tool sử dụng
    /// </summary>
    public Guid? ToolId { get; set; }
    
    /// <summary>
    /// Vật tư chính sử dụng
    /// </summary>
    public Guid? MaterialId { get; set; }
    
    /// <summary>
    /// Chu kỳ thực tế (giây)
    /// </summary>
    public decimal? CycleTime { get; set; }
    
    /// <summary>
    /// Số lượng kế hoạch
    /// </summary>
    public int PlannedQuantity { get; set; }
    
    /// <summary>
    /// Số lượng đã hoàn thành
    /// </summary>
    public int CompletedQuantity { get; set; }
    
    /// <summary>
    /// Trạng thái: Pending, InProgress, Completed, Paused
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// Ngày bắt đầu kế hoạch
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }
    
    /// <summary>
    /// Ngày kết thúc kế hoạch
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }
    
    /// <summary>
    /// Ngày bắt đầu thực tế
    /// </summary>
    public DateTime? ActualStartDate { get; set; }
    
    /// <summary>
    /// Ngày kết thúc thực tế
    /// </summary>
    public DateTime? ActualEndDate { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Thứ tự công đoạn
    /// </summary>
    public int SequenceOrder { get; set; }
    
    /// <summary>
    /// Người phụ trách (PMC)
    /// </summary>
    public string? AssignedTo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
    public virtual ProcessMethod? ProcessMethod { get; set; }
    public virtual Machine? Machine { get; set; }
    public virtual Tool? Tool { get; set; }
    public virtual Material? Material { get; set; }
    public virtual ICollection<MappingPOProduction> MappingPOProductions { get; set; } = new List<MappingPOProduction>();
    public virtual ICollection<ProductionOperationMaterial> ProductionOperationMaterials { get; set; } = new List<ProductionOperationMaterial>();
}










namespace SmartFactory.Application.Entities;

/// <summary>
/// PMC Row - Represents a planning row in the PMC week
/// Each row contains planning data for a specific product-component-customer-plantype combination
/// </summary>
public class PMCRow
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to PMC Week
    /// </summary>
    public Guid PMCWeekId { get; set; }
    
    /// <summary>
    /// Product code (Mã vật liệu)
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Component/Part name (Tên linh kiện)
    /// </summary>
    public string ComponentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer ID
    /// </summary>
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// Customer name for display
    /// </summary>
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// Plan type: REQUIREMENT (Yêu cầu KH/PO数量), PRODUCTION (Kế hoạch SX/生产计划), CLAMP (Kẹp/呼叫按)
    /// </summary>
    public string PlanType { get; set; } = string.Empty;
    
    /// <summary>
    /// Display order for UI grouping
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Total value (Tổng) - manually entered by user
    /// </summary>
    public decimal? TotalValue { get; set; }
    
    /// <summary>
    /// Row group identifier (used for merging cells in UI)
    /// Format: ProductCode_ComponentName
    /// </summary>
    public string RowGroup { get; set; } = string.Empty;
    
    /// <summary>
    /// Notes for this row
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual PMCWeek? PMCWeek { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<PMCCell> Cells { get; set; } = new List<PMCCell>();
}

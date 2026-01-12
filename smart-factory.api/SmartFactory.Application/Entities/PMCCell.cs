namespace SmartFactory.Application.Entities;

/// <summary>
/// PMC Cell - Represents a single cell value for a specific date in the PMC planning
/// </summary>
public class PMCCell
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to PMC Row
    /// </summary>
    public Guid PMCRowId { get; set; }
    
    /// <summary>
    /// Work date (one of the 6 days in the week)
    /// </summary>
    public DateTime WorkDate { get; set; }
    
    /// <summary>
    /// Planning value for this date
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Is this cell editable by user?
    /// REQUIREMENT rows are usually auto-calculated from PO
    /// PRODUCTION and CLAMP rows are user-editable
    /// </summary>
    public bool IsEditable { get; set; } = true;
    
    /// <summary>
    /// Background color for cell (e.g., "yellow", "green", null)
    /// </summary>
    public string? BackgroundColor { get; set; }
    
    /// <summary>
    /// Cell notes/remarks
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
    public virtual PMCRow? PMCRow { get; set; }
}

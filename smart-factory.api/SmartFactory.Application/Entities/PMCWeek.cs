namespace SmartFactory.Application.Entities;

/// <summary>
/// PMC Week - Represents a production planning week (Monday to Saturday)
/// Each week can have multiple versions (snapshots)
/// </summary>
public class PMCWeek
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Week start date (Monday)
    /// </summary>
    public DateTime WeekStartDate { get; set; }
    
    /// <summary>
    /// Week end date (Saturday)
    /// </summary>
    public DateTime WeekEndDate { get; set; }
    
    /// <summary>
    /// Version number (1, 2, 3, ...)
    /// Each save creates a new version (snapshot)
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Week name for display (e.g., "Week 1 - Jan 2026")
    /// </summary>
    public string WeekName { get; set; } = string.Empty;
    
    /// <summary>
    /// Is this the latest active version for this week?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Status: DRAFT, APPROVED, LOCKED
    /// </summary>
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// Notes for this version
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Created by user
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual User? Creator { get; set; }
    public virtual ICollection<PMCRow> Rows { get; set; } = new List<PMCRow>();
}

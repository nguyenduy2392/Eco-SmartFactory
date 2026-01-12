namespace SmartFactory.Application.DTOs;

/// <summary>
/// DTO for PMC Week data
/// </summary>
public class PMCWeekDto
{
    public Guid Id { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int Version { get; set; }
    public string WeekName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Array of 6 dates (Monday to Saturday) for display
    /// </summary>
    public List<DateTime> WeekDates { get; set; } = new();
    
    /// <summary>
    /// All planning rows for this week
    /// </summary>
    public List<PMCRowDto> Rows { get; set; } = new();
}

/// <summary>
/// DTO for PMC Row data
/// </summary>
public class PMCRowDto
{
    public Guid Id { get; set; }
    public Guid PMCWeekId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? TotalValue { get; set; }
    public string RowGroup { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    /// <summary>
    /// Plan type display name
    /// REQUIREMENT -> "Yêu cầu KH / PO数量"
    /// PRODUCTION -> "Kế hoạch SX / 生产计划"
    /// CLAMP -> "Kẹp / 呼叫按"
    /// </summary>
    public string PlanTypeDisplay { get; set; } = string.Empty;
    
    /// <summary>
    /// Cell values for each date
    /// </summary>
    public List<PMCCellDto> Cells { get; set; } = new();
}

/// <summary>
/// DTO for PMC Cell data
/// </summary>
public class PMCCellDto
{
    public Guid Id { get; set; }
    public Guid PMCRowId { get; set; }
    public DateTime WorkDate { get; set; }
    public decimal Value { get; set; }
    public bool IsEditable { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for creating a new PMC week
/// </summary>
public class CreatePMCWeekRequest
{
    /// <summary>
    /// Week start date (Monday). If not provided, system will calculate next week Monday.
    /// </summary>
    public DateTime? WeekStartDate { get; set; }
    
    /// <summary>
    /// Notes for this version
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Should copy data from previous week?
    /// </summary>
    public bool CopyFromPreviousWeek { get; set; } = false;
}

/// <summary>
/// Request DTO for saving PMC week (creates new version)
/// </summary>
public class SavePMCWeekRequest
{
    public Guid PMCWeekId { get; set; }
    
    /// <summary>
    /// Notes for this new version
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// All row data to save
    /// </summary>
    public List<SavePMCRowRequest> Rows { get; set; } = new();
}

/// <summary>
/// Request DTO for saving a single PMC row
/// </summary>
public class SavePMCRowRequest
{
    public Guid? Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public decimal? TotalValue { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Cell values for each date
    /// Key: WorkDate (yyyy-MM-dd), Value: cell value
    /// </summary>
    public Dictionary<string, decimal> CellValues { get; set; } = new();
}

/// <summary>
/// Response DTO for listing PMC weeks
/// </summary>
public class PMCWeekListItemDto
{
    public Guid Id { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int Version { get; set; }
    public string WeekName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalRows { get; set; }
}

/// <summary>
/// Constants for PMC plan types
/// </summary>
public static class PMCPlanTypes
{
    public const string Requirement = "REQUIREMENT";
    public const string Production = "PRODUCTION";
    public const string Clamp = "CLAMP";
    
    public static string GetDisplayName(string planType)
    {
        return planType switch
        {
            Requirement => "Yêu cầu KH / PO数量",
            Production => "Kế hoạch SX / 生产计划",
            Clamp => "Kẹp / 呼叫按",
            _ => planType
        };
    }
}

/// <summary>
/// Constants for PMC statuses
/// </summary>
public static class PMCStatus
{
    public const string Draft = "DRAFT";
    public const string Approved = "APPROVED";
    public const string Locked = "LOCKED";
}

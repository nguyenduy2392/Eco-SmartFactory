namespace SmartFactory.Application.DTOs;

/// <summary>
/// Process BOM DTO
/// </summary>
public class ProcessBOMDto
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public Guid ProcessingTypeId { get; set; }
    public string ProcessingTypeName { get; set; } = string.Empty;
    public string Version { get; set; } = "V1";
    public string Status { get; set; } = "ACTIVE";
    public DateTime? EffectiveDate { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    public List<ProcessBOMDetailDto>? BOMDetails { get; set; }
}

public class ProcessBOMDetailDto
{
    public Guid Id { get; set; }
    public Guid ProcessBOMId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public decimal ScrapRate { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? ProcessStep { get; set; }
    public string? Notes { get; set; }
    public int SequenceOrder { get; set; }
}

public class CreateProcessBOMRequest
{
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public List<CreateProcessBOMDetailRequest> Details { get; set; } = new();
}

public class CreateProcessBOMDetailRequest
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public decimal ScrapRate { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public string? ProcessStep { get; set; }
    public string? Notes { get; set; }
    public int SequenceOrder { get; set; }
}

public class UpdateProcessBOMRequest
{
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public List<CreateProcessBOMDetailRequest>? Details { get; set; }
}

/// <summary>
/// Availability Check Request
/// </summary>
public class AvailabilityCheckRequest
{
    // For PO-based check
    public Guid? PurchaseOrderId { get; set; }
    public int? PlannedQuantity { get; set; }
    
    // For component-based check (not PO-based)
    public Guid? PartId { get; set; }
    public Guid? ProcessingTypeId { get; set; }
    public int? Quantity { get; set; }
}

/// <summary>
/// Availability Check Result
/// </summary>
public class AvailabilityCheckResult
{
    /// <summary>
    /// Overall status: PASS, FAIL, WARNING
    /// </summary>
    public string OverallStatus { get; set; } = "PASS";
    
    /// <summary>
    /// PO ID being checked (for PO-based check)
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Planned quantity (for PO-based check)
    /// </summary>
    public int? PlannedQuantity { get; set; }
    
    /// <summary>
    /// Part ID (for component-based check)
    /// </summary>
    public Guid? PartId { get; set; }
    
    /// <summary>
    /// Processing Type ID (for component-based check)
    /// </summary>
    public Guid? ProcessingTypeId { get; set; }
    
    /// <summary>
    /// Quantity (for component-based check)
    /// </summary>
    public int? Quantity { get; set; }
    
    /// <summary>
    /// Part-level results
    /// </summary>
    public List<PartAvailabilityDetail> PartDetails { get; set; } = new();
    
    /// <summary>
    /// Check timestamp
    /// </summary>
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Part-level availability detail
/// </summary>
public class PartAvailabilityDetail
{
    public Guid PartId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string ProcessingType { get; set; } = string.Empty;
    public string ProcessingTypeName { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; }
    
    /// <summary>
    /// Whether this part can be produced (has ACTIVE BOM)
    /// </summary>
    public bool CanProduce { get; set; }
    
    /// <summary>
    /// Severity: OK, WARNING, CRITICAL
    /// </summary>
    public string Severity { get; set; } = "OK";
    
    /// <summary>
    /// BOM Version if available
    /// </summary>
    public string? BOMVersion { get; set; }
    
    /// <summary>
    /// Whether ACTIVE BOM exists
    /// </summary>
    public bool HasActiveBOM { get; set; }
}



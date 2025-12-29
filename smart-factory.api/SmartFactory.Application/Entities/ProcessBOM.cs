namespace SmartFactory.Application.Entities;

/// <summary>
/// Process BOM - Bill of Materials per (Part + Processing Type)
/// Defines material consumption per 1 PCS of a part
/// BOM belongs to HOW TO MAKE, not HOW TO CHARGE
/// BOM is independent from PO and pricing
/// </summary>
public class ProcessBOM
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Linh kiện
    /// </summary>
    public Guid PartId { get; set; }
    
    /// <summary>
    /// Loại gia công (EP_NHUA, PHUN_IN, LAP_RAP)
    /// </summary>
    public Guid ProcessingTypeId { get; set; }
    
    /// <summary>
    /// BOM Version (V1, V2, V3...)
    /// </summary>
    public string Version { get; set; } = "V1";
    
    /// <summary>
    /// Status: ACTIVE, INACTIVE
    /// Only ONE BOM per (Part + ProcessingType) can be ACTIVE
    /// Creating new BOM version automatically sets old version to INACTIVE
    /// </summary>
    public string Status { get; set; } = "ACTIVE";
    
    /// <summary>
    /// Ngày hiệu lực của BOM
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
    
    /// <summary>
    /// BOM Name / Description
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Part Part { get; set; } = null!;
    public virtual ProcessingType ProcessingType { get; set; } = null!;
    public virtual ICollection<ProcessBOMDetail> BOMDetails { get; set; } = new List<ProcessBOMDetail>();
}



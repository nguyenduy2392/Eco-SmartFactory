namespace SmartFactory.Application.DTOs;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Version: V0, V1, V2...
    /// </summary>
    public string Version { get; set; } = "V0";
    
    /// <summary>
    /// Processing Type: EP_NHUA, LAP_RAP, PHUN_IN
    /// </summary>
    public string? ProcessingType { get; set; }
    
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    /// <summary>
    /// Status: DRAFT, APPROVED_FOR_PMC, LOCKED
    /// </summary>
    public string Status { get; set; } = "DRAFT";
    
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? OriginalPOId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsMaterialFullyReceived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // Related data
    public List<POProductDto>? Products { get; set; }
    public List<POOperationDto>? Operations { get; set; }
    public List<POMaterialBaselineDto>? MaterialBaselines { get; set; }
    public List<PurchaseOrderMaterialDto>? PurchaseOrderMaterials { get; set; }
}

public class PurchaseOrderListDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Version { get; set; } = "V0";
    public string? ProcessingType { get; set; }
    public DateTime PODate { get; set; }
    public string Status { get; set; } = "DRAFT";
    public decimal TotalAmount { get; set; }
    public int ProductCount { get; set; }
    public bool IsMaterialFullyReceived { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public string PONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? TemplateType { get; set; }
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePOProductRequest>? Products { get; set; }
}

public class UpdatePurchaseOrderRequest
{
    public Guid CustomerId { get; set; }
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = "New";
    public string? Notes { get; set; }
}

public class UpdateGeneralInfoRequest
{
    public string? PONumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? ProcessingType { get; set; }
    public DateTime? PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePOOperationRequest
{
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? ContractUnitPrice { get; set; } // Đơn giá hợp đồng (PCS) - for PHUN_IN
    public int Quantity { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Notes { get; set; }
    // Product and Part codes for updating relationships
    public string? ProductCode { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
    // ÉP NHỰA specific fields
    public string? ModelNumber { get; set; }
    public string? Material { get; set; }
    public string? ColorCode { get; set; }
    public string? Color { get; set; }
    public int? CavityQuantity { get; set; }
    public int? Set { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? TotalWeight { get; set; }
    public string? MachineType { get; set; }
    public decimal? RequiredMaterial { get; set; }
    public decimal? RequiredColor { get; set; }
}

public class ClonePOVersionRequest
{
    public Guid OriginalPOId { get; set; }
    public string? Notes { get; set; }
}

public class POMaterialBaselineDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal CommittedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? PartCode { get; set; }
    public string? Notes { get; set; }
}





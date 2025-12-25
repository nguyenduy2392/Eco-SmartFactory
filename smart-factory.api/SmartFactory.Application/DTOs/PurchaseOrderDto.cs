namespace SmartFactory.Application.DTOs;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VersionType { get; set; } = "ORIGINAL";
    public string? TemplateType { get; set; }
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = "New";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? OriginalPOId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // Related data
    public List<POProductDto>? Products { get; set; }
    public List<POOperationDto>? Operations { get; set; }
}

public class PurchaseOrderListDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VersionType { get; set; } = "ORIGINAL";
    public DateTime PODate { get; set; }
    public string Status { get; set; } = "New";
    public decimal TotalAmount { get; set; }
    public int ProductCount { get; set; }
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

public class ClonePOVersionRequest
{
    public Guid OriginalPOId { get; set; }
    public string NewVersionType { get; set; } = "FINAL";
    public string? Notes { get; set; }
}



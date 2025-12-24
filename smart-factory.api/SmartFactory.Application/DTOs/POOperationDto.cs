namespace SmartFactory.Application.DTOs;

public class POOperationDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid PartId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public Guid ProcessingTypeId { get; set; }
    public string ProcessingTypeName { get; set; } = string.Empty;
    public Guid? ProcessMethodId { get; set; }
    public string? ProcessMethodName { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
    public int SequenceOrder { get; set; }
}

public class CreatePOOperationRequest
{
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public Guid? ProcessMethodId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
    public int SequenceOrder { get; set; }
}


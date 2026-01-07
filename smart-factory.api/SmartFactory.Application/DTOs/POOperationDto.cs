namespace SmartFactory.Application.DTOs;

public class POOperationDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid PartId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public Guid? ProductId { get; set; } // Product that contains this part
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public string ProcessingTypeName { get; set; } = string.Empty;
    public Guid? ProcessMethodId { get; set; }
    public string? ProcessMethodName { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? ContractUnitPrice { get; set; } // Đơn giá hợp đồng (PCS) - for PHUN_IN
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
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
    public string? Notes { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int SequenceOrder { get; set; }
}

public class CreatePOOperationRequest
{
    public Guid? PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public Guid? ProcessMethodId { get; set; }
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
    public int SequenceOrder { get; set; }
    // Product and Part codes for creating relationships
    public string? ProductCode { get; set; }
    public string? PartCode { get; set; }
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




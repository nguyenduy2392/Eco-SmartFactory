namespace SmartFactory.Application.DTOs;

public class PurchaseOrderMaterialDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialType { get; set; }
    public decimal PlannedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Notes { get; set; }
}

public class MaterialReceiptHistoryDto
{
    public Guid Id { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PONumber { get; set; }
    public Guid MaterialReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
}

public class StockInRequest
{
    /// <summary>
    /// PO liên quan (nullable - có thể không gắn PO)
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Chủ hàng
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Kho nhập
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Ngày nhập kho
    /// </summary>
    public DateTime ReceiptDate { get; set; }
    
    /// <summary>
    /// Số phiếu nhập
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Danh sách nguyên vật liệu nhập kho
    /// </summary>
    public List<StockInMaterialItem> Materials { get; set; } = new();
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
}

public class StockInMaterialItem
{
    public Guid MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? PurchasePOCode { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePOMaterialStatusRequest
{
    public bool IsMaterialFullyReceived { get; set; }
}

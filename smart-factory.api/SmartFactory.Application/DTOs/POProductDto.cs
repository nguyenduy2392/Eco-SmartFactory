namespace SmartFactory.Application.DTOs;

public class POProductDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreatePOProductRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}




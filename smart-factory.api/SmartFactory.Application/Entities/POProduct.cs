namespace SmartFactory.Application.Entities;

/// <summary>
/// Sản phẩm trong PO - Mối quan hệ nhiều-nhiều giữa PO và Product
/// </summary>
public class POProduct
{
    public Guid Id { get; set; }
    
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Số lượng sản phẩm trong PO
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Đơn giá sản phẩm (nếu tính theo sản phẩm)
    /// </summary>
    public decimal? UnitPrice { get; set; }
    
    /// <summary>
    /// Tổng tiền cho sản phẩm này
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}



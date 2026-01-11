namespace SmartFactory.Application.Entities;

/// <summary>
/// Phiếu nhập kho nguyên vật liệu - Quản lý các lần nhập kho thực tế từ chủ hàng
/// File Excel NHAP_NGUYEN_VAT_LIEU từ chủ hàng gửi cho Hải Tân sẽ tạo các MaterialReceipt này
/// Một Material có thể có nhiều lần nhập kho khác nhau
/// </summary>
public class MaterialReceipt
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Chủ hàng cung cấp nguyên vật liệu
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Nguyên vật liệu được nhập
    /// </summary>
    public Guid MaterialId { get; set; }
    
    /// <summary>
    /// Kho nhập
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Số lượng nhập
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Đơn vị tính
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lô (Batch Number) - Tùy chọn
    /// </summary>
    public string? BatchNumber { get; set; }
    
    /// <summary>
    /// Ngày nhập kho
    /// </summary>
    public DateTime ReceiptDate { get; set; }
    
    /// <summary>
    /// Mã nhà cung cấp (nếu có - có thể là chủ hàng hoặc nhà cung cấp khác)
    /// </summary>
    public string? SupplierCode { get; set; }
    
    /// <summary>
    /// Mã PO mua hàng (nếu có - có thể là PO mua từ nhà cung cấp)
    /// </summary>
    public string? PurchasePOCode { get; set; }
    
    /// <summary>
    /// Số phiếu nhập (Import Slip Number)
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Ghi chú (có thể chứa thông tin "Nhập đầu kỳ tháng X")
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Trạng thái: PENDING, RECEIVED, CANCELLED
    /// </summary>
    public string Status { get; set; } = "PENDING";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}


namespace SmartFactory.Application.Entities;

/// <summary>
/// Công đoạn theo PO (PO Operation / Charge Operation)
/// Là đơn vị gia công được dùng để TÍNH TIỀN, theo hợp đồng với chủ hàng
/// KHÔNG gắn với tool, máy, nhân sự cụ thể
/// </summary>
public class POOperation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// PO chứa công đoạn này
    /// </summary>
    public Guid PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Linh kiện được gia công
    /// </summary>
    public Guid PartId { get; set; }
    
    /// <summary>
    /// Loại hình gia công (ÉP/SƠN/LẮP)
    /// </summary>
    public Guid ProcessingTypeId { get; set; }
    
    /// <summary>
    /// Phương pháp gia công (nullable - có thể chỉ ghi tổng quát)
    /// </summary>
    public Guid? ProcessMethodId { get; set; }
    
    /// <summary>
    /// Tên công đoạn (ví dụ: "Phun kẹp", "In sơn", "Lắp ráp tổng")
    /// </summary>
    public string OperationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lần gia công / Charge Count (加工次数)
    /// Ví dụ: Phun kẹp × 4 công đoạn
    /// </summary>
    public int ChargeCount { get; set; } = 1;
    
    /// <summary>
    /// Đơn giá (VND hoặc USD)
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Số lượng sản phẩm (quantity)
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Tổng tiền = ChargeCount × UnitPrice × Quantity
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Vị trí phun (cho loại PHUN IN)
    /// </summary>
    public string? SprayPosition { get; set; }
    
    /// <summary>
    /// Nội dung in (cho loại PHUN IN)
    /// </summary>
    public string? PrintContent { get; set; }
    
    /// <summary>
    /// Chu kỳ (giây) - cho loại ÉP NHỰA
    /// </summary>
    public decimal? CycleTime { get; set; }
    
    /// <summary>
    /// Nội dung lắp ráp (cho loại LẮP RÁP)
    /// </summary>
    public string? AssemblyContent { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Thứ tự công đoạn
    /// </summary>
    public int SequenceOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
    public virtual ProcessingType ProcessingType { get; set; } = null!;
    public virtual ProcessMethod? ProcessMethod { get; set; }
    public virtual ICollection<MappingPOProduction> MappingPOProductions { get; set; } = new List<MappingPOProduction>();
}


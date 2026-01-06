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
    /// Linh kiện được gia công (nullable cho LAP_RAP - đôi lúc không cần linh kiện)
    /// </summary>
    public Guid? PartId { get; set; }
    
    /// <summary>
    /// Sản phẩm được gia công (nullable - có thể chỉ có Product mà chưa xác định Part cụ thể)
    /// </summary>
    public Guid? ProductId { get; set; }
    
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
    
    // ÉP NHỰA specific fields
    /// <summary>
    /// Mã số mẫu/khuôn (模号/款号)
    /// </summary>
    public string? ModelNumber { get; set; }
    
    /// <summary>
    /// Loại vật liệu (胶料)
    /// </summary>
    public string? Material { get; set; }
    
    /// <summary>
    /// Mã màu (色份遍号)
    /// </summary>
    public string? ColorCode { get; set; }
    
    /// <summary>
    /// Màu sắc (颜色)
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Số lòng khuôn (莫腔量)
    /// </summary>
    public int? CavityQuantity { get; set; }
    
    /// <summary>
    /// Số bộ (套Bộ)
    /// </summary>
    public int? Set { get; set; }
    
    /// <summary>
    /// Trọng lượng tịnh (淨重)
    /// </summary>
    public decimal? NetWeight { get; set; }
    
    /// <summary>
    /// Tổng trọng lượng (总重量)
    /// </summary>
    public decimal? TotalWeight { get; set; }
    
    /// <summary>
    /// Loại máy ép (晚機型號)
    /// </summary>
    public string? MachineType { get; set; }
    
    /// <summary>
    /// Lượng nhựa cần (需要胶料)
    /// </summary>
    public decimal? RequiredMaterial { get; set; }
    
    /// <summary>
    /// Lượng màu cần (需要色份)
    /// </summary>
    public decimal? RequiredColor { get; set; }
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Ngày hoàn thành (Completion Date)
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>
    /// Thứ tự công đoạn
    /// </summary>
    public int SequenceOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Part? Part { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ProcessingType ProcessingType { get; set; } = null!;
    public virtual ProcessMethod? ProcessMethod { get; set; }
    public virtual ICollection<MappingPOProduction> MappingPOProductions { get; set; } = new List<MappingPOProduction>();
}








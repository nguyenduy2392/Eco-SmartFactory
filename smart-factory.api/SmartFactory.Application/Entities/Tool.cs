namespace SmartFactory.Application.Entities;

/// <summary>
/// Tool / Công cụ - Thứ không tiêu hao, dùng để gia công
/// Ví dụ: Khuôn ép, Kẹp sơn, Đầu in, Jig lắp ráp
/// Thuộc sở hữu chủ hàng, Hải Tân quản lý sử dụng - bàn giao - hoàn trả
/// </summary>
public class Tool
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Mã tool (TOOL-005, MOLD-A88)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên tool
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại tool: Mold (Khuôn), Clamp (Kẹp), Jig (Đồ gá), PrintHead (Đầu in)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Chủ sở hữu (Customer)
    /// </summary>
    public Guid? OwnerId { get; set; }
    
    /// <summary>
    /// Trạng thái: Available, InUse, Maintenance, Returned
    /// </summary>
    public string Status { get; set; } = "Available";
    
    /// <summary>
    /// Ngày nhận tool
    /// </summary>
    public DateTime? ReceivedDate { get; set; }
    
    /// <summary>
    /// Ngày trả tool
    /// </summary>
    public DateTime? ReturnedDate { get; set; }
    
    /// <summary>
    /// Số lần sử dụng
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// Tuổi thọ ước tính (số lần)
    /// </summary>
    public int? EstimatedLifespan { get; set; }
    
    /// <summary>
    /// Vị trí lưu trữ
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer? Owner { get; set; }
    public virtual ICollection<ProductionOperation> ProductionOperations { get; set; } = new List<ProductionOperation>();
}


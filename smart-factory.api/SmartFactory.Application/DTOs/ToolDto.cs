namespace SmartFactory.Application.DTOs;

public class ToolDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public int UsageCount { get; set; }
    public int? EstimatedLifespan { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateToolRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? OwnerId { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int? EstimatedLifespan { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class UpdateToolRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? OwnerId { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public int? EstimatedLifespan { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}


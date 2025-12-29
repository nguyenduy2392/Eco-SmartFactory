namespace SmartFactory.Application.DTOs;

public class PartDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Position { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePartRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string? Position { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public string? Description { get; set; }
}






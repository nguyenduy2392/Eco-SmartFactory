namespace SmartFactory.Application.DTOs;

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMaterialRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Description { get; set; }
}

public class UpdateMaterialRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}



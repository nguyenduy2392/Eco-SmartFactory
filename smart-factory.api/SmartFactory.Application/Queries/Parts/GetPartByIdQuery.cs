using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Queries.Parts;

public class GetPartByIdQuery : IRequest<PartDetailDto?>
{
    public Guid PartId { get; set; }
    public Guid PurchaseOrderId { get; set; }
}

public class PartDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Position { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Draft";
    
    // Processes grouped by ProcessingType
    public List<ProcessTypeDto> Processes { get; set; } = new();
}

public class ProcessTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "pi-box";
    public string Color { get; set; } = "blue";
    public List<ProductionOperationDto> Stages { get; set; } = new();
}

public class ProductionOperationDto
{
    public Guid Id { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string? MachineId { get; set; }
    public string? MachineName { get; set; }
    public decimal? CycleTime { get; set; }
    public int SequenceOrder { get; set; }
    public string Status { get; set; } = "Pending";
    public List<OperationMaterialDto> Materials { get; set; } = new();
    public List<OperationToolDto> Tools { get; set; } = new();
}

public class OperationMaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "kg";
}

public class OperationToolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ToolId { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class GetPartByIdQueryHandler : IRequestHandler<GetPartByIdQuery, PartDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetPartByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PartDetailDto?> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await _context.Parts
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PartId, cancellationToken);

        if (part == null)
        {
            return null;
        }

        // Get ProductionOperations with all related data
        var productionOperations = await _context.ProductionOperations
            .Where(po => po.PartId == request.PartId && po.PurchaseOrderId == request.PurchaseOrderId)
            .Include(po => po.ProcessMethod)
                .ThenInclude(pm => pm.ProcessingType)
            .Include(po => po.Machine)
            .Include(po => po.Tool)
            .Include(po => po.Material)
            .Include(po => po.ProductionOperationMaterials)
                .ThenInclude(pom => pom.Material)
            .OrderBy(po => po.SequenceOrder)
            .ToListAsync(cancellationToken);

        // Get all ProcessingTypes to group operations
        var processingTypes = await _context.ProcessingTypes
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.DisplayOrder)
            .ToListAsync(cancellationToken);

        // Group operations by ProcessingType
        var processGroups = new List<ProcessTypeDto>();
        
        foreach (var processingType in processingTypes)
        {
            var operationsForType = productionOperations
                .Where(po => po.ProcessMethod != null && 
                            po.ProcessMethod.ProcessingTypeId == processingType.Id)
                .Select(po => new ProductionOperationDto
                {
                    Id = po.Id,
                    OperationName = po.OperationName,
                    MachineId = po.Machine?.Code,
                    MachineName = po.Machine?.Name,
                    CycleTime = po.CycleTime,
                    SequenceOrder = po.SequenceOrder,
                    Status = po.Status,
                    Materials = po.ProductionOperationMaterials.Select(pom => new OperationMaterialDto
                    {
                        Id = pom.Material.Id,
                        Name = pom.Material.Name,
                        Code = pom.Material.Code,
                        Quantity = pom.QuantityRequired,
                        Unit = pom.Material.Unit ?? "kg"
                    }).ToList(),
                    Tools = po.Tool != null ? new List<OperationToolDto>
                    {
                        new OperationToolDto
                        {
                            Id = po.Tool.Id,
                            Name = po.Tool.Name,
                            ToolId = po.Tool.Code,
                            Code = po.Tool.Code
                        }
                    } : new List<OperationToolDto>()
                })
                .OrderBy(op => op.SequenceOrder)
                .ToList();

            if (operationsForType.Any())
            {
                processGroups.Add(new ProcessTypeDto
                {
                    Id = processingType.Id,
                    Name = processingType.Name,
                    Code = processingType.Code,
                    Description = processingType.Description,
                    Icon = GetIconForProcessingType(processingType.Code),
                    Color = GetColorForProcessingType(processingType.Code),
                    Stages = operationsForType
                });
            }
        }

        // Also include operations without ProcessMethod (standalone)
        var standaloneOperations = productionOperations
            .Where(po => po.ProcessMethod == null)
            .Select(po => new ProductionOperationDto
            {
                Id = po.Id,
                OperationName = po.OperationName,
                MachineId = po.Machine?.Code,
                MachineName = po.Machine?.Name,
                CycleTime = po.CycleTime,
                SequenceOrder = po.SequenceOrder,
                Status = po.Status,
                Materials = po.ProductionOperationMaterials.Select(pom => new OperationMaterialDto
                {
                    Id = pom.Material.Id,
                    Name = pom.Material.Name,
                    Code = pom.Material.Code,
                    Quantity = pom.QuantityRequired,
                    Unit = pom.Material.Unit ?? "kg"
                }).ToList(),
                Tools = po.Tool != null ? new List<OperationToolDto>
                {
                    new OperationToolDto
                    {
                        Id = po.Tool.Id,
                        Name = po.Tool.Name,
                        ToolId = po.Tool.Code,
                        Code = po.Tool.Code
                    }
                } : new List<OperationToolDto>()
            })
            .OrderBy(op => op.SequenceOrder)
            .ToList();

        // If there are standalone operations, add them to a default process
        if (standaloneOperations.Any())
        {
            processGroups.Add(new ProcessTypeDto
            {
                Id = Guid.Empty,
                Name = "Khác",
                Code = "OTHER",
                Description = "Các công đoạn khác",
                Icon = "pi-cog",
                Color = "gray",
                Stages = standaloneOperations
            });
        }

        return new PartDetailDto
        {
            Id = part.Id,
            Code = part.Code,
            Name = part.Name,
            ProductId = part.ProductId,
            ProductName = part.Product.Name,
            ProductCode = part.Product.Code,
            Position = part.Position,
            Material = part.Material,
            Color = part.Color,
            Weight = part.Weight,
            Description = part.Description,
            ImageUrl = part.ImageUrl,
            IsActive = part.IsActive,
            CreatedAt = part.CreatedAt,
            Status = part.IsActive ? "In Production" : "Draft",
            Processes = processGroups
        };
    }

    private string GetIconForProcessingType(string code)
    {
        return code.ToUpper() switch
        {
            "EP" or "EP_NHUA" => "pi-box",
            "SON" or "PAINTING" => "pi-palette",
            "LAP_RAP" or "ASSEMBLY" => "pi-wrench",
            _ => "pi-cog"
        };
    }

    private string GetColorForProcessingType(string code)
    {
        return code.ToUpper() switch
        {
            "EP" or "EP_NHUA" => "blue",
            "SON" or "PAINTING" => "orange",
            "LAP_RAP" or "ASSEMBLY" => "purple",
            _ => "gray"
        };
    }
}


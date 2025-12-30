using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.ProcessBOM;

/// <summary>
/// Command to create Process BOM
/// PHASE 1: BOM defines material consumption per 1 PCS
/// Creating new BOM version automatically sets old version to INACTIVE
/// </summary>
public class CreateProcessBOMCommand : IRequest<ProcessBOMDto>
{
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public List<CreateProcessBOMDetailRequest> Details { get; set; } = new();
}

public class CreateProcessBOMCommandHandler : IRequestHandler<CreateProcessBOMCommand, ProcessBOMDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateProcessBOMCommandHandler> _logger;

    public CreateProcessBOMCommandHandler(
        ApplicationDbContext context,
        ILogger<CreateProcessBOMCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProcessBOMDto> Handle(CreateProcessBOMCommand request, CancellationToken cancellationToken)
    {
        // Validate Part exists
        var part = await _context.Parts
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PartId, cancellationToken);
        if (part == null)
        {
            throw new Exception($"Part with ID {request.PartId} not found");
        }

        // Validate ProcessingType exists
        var processingType = await _context.ProcessingTypes
            .FirstOrDefaultAsync(pt => pt.Id == request.ProcessingTypeId, cancellationToken);
        if (processingType == null)
        {
            throw new Exception($"ProcessingType with ID {request.ProcessingTypeId} not found");
        }

        // Validate BOM must have at least one material line
        if (!request.Details.Any())
        {
            throw new Exception("BOM must contain at least one material line");
        }

        // PHASE 1: Deactivate old ACTIVE BOM for this (Part + ProcessingType)
        var existingActiveBOM = await _context.ProcessBOMs
            .Where(b => b.PartId == request.PartId 
                && b.ProcessingTypeId == request.ProcessingTypeId 
                && b.Status == "ACTIVE")
            .ToListAsync(cancellationToken);

        if (existingActiveBOM.Any())
        {
            foreach (var oldBOM in existingActiveBOM)
            {
                oldBOM.Status = "INACTIVE";
                oldBOM.UpdatedAt = DateTime.UtcNow;
            }
            _logger.LogInformation("Deactivated {Count} existing BOM(s) for Part {PartCode} + ProcessingType {ProcessingType}",
                existingActiveBOM.Count, part.Code, processingType.Name);
        }

        // Determine version number
        var maxVersion = await _context.ProcessBOMs
            .Where(b => b.PartId == request.PartId && b.ProcessingTypeId == request.ProcessingTypeId)
            .Select(b => b.Version)
            .ToListAsync(cancellationToken);

        int nextVersionNumber = 1;
        if (maxVersion.Any())
        {
            // Parse version numbers (V1, V2, V3...)
            var versionNumbers = maxVersion
                .Where(v => v.StartsWith("V") && int.TryParse(v.Substring(1), out _))
                .Select(v => int.Parse(v.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();
            nextVersionNumber = versionNumbers + 1;
        }

        var versionLabel = $"V{nextVersionNumber}";

        // Create new BOM
        var bom = new Entities.ProcessBOM
        {
            PartId = request.PartId,
            ProcessingTypeId = request.ProcessingTypeId,
            Version = versionLabel,
            Status = "ACTIVE",
            EffectiveDate = request.EffectiveDate,
            Name = request.Name,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProcessBOMs.Add(bom);
        await _context.SaveChangesAsync(cancellationToken);

        // Create BOM Details
        foreach (var detailRequest in request.Details)
        {
            // Validate scrap rate
            if (detailRequest.ScrapRate < 0)
            {
                throw new Exception($"Scrap rate must be >= 0 for material {detailRequest.MaterialCode}");
            }

            var detail = new ProcessBOMDetail
            {
                ProcessBOMId = bom.Id,
                MaterialCode = detailRequest.MaterialCode,
                MaterialName = detailRequest.MaterialName,
                QuantityPerUnit = detailRequest.QuantityPerUnit,
                ScrapRate = detailRequest.ScrapRate,
                Unit = detailRequest.Unit,
                ProcessStep = detailRequest.ProcessStep,
                Notes = detailRequest.Notes,
                SequenceOrder = detailRequest.SequenceOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProcessBOMDetails.Add(detail);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created BOM {Version} for Part {PartCode} + ProcessingType {ProcessingType} with {DetailCount} materials",
            versionLabel, part.Code, processingType.Name, request.Details.Count);

        // Load details for response
        var bomDetails = await _context.ProcessBOMDetails
            .Where(d => d.ProcessBOMId == bom.Id)
            .OrderBy(d => d.SequenceOrder)
            .ToListAsync(cancellationToken);

        return new ProcessBOMDto
        {
            Id = bom.Id,
            PartId = bom.PartId,
            PartCode = part.Code,
            PartName = part.Name,
            ProcessingTypeId = bom.ProcessingTypeId,
            ProcessingTypeName = processingType.Name,
            Version = bom.Version,
            Status = bom.Status,
            EffectiveDate = bom.EffectiveDate,
            Name = bom.Name,
            Notes = bom.Notes,
            CreatedAt = bom.CreatedAt,
            BOMDetails = bomDetails.Select(d => new ProcessBOMDetailDto
            {
                Id = d.Id,
                ProcessBOMId = d.ProcessBOMId,
                MaterialCode = d.MaterialCode,
                MaterialName = d.MaterialName,
                QuantityPerUnit = d.QuantityPerUnit,
                ScrapRate = d.ScrapRate,
                Unit = d.Unit,
                ProcessStep = d.ProcessStep,
                Notes = d.Notes,
                SequenceOrder = d.SequenceOrder
            }).ToList()
        };
    }
}




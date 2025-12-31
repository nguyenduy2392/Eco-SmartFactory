using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.ProcessBOM;

/// <summary>
/// Query to get ACTIVE BOM for a (Part + ProcessingType)
/// PHASE 1: Only ONE BOM per (Part + ProcessingType) can be ACTIVE
/// </summary>
public class GetActiveBOMByPartAndTypeQuery : IRequest<ProcessBOMDto?>
{
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
}

public class GetActiveBOMByPartAndTypeQueryHandler : IRequestHandler<GetActiveBOMByPartAndTypeQuery, ProcessBOMDto?>
{
    private readonly ApplicationDbContext _context;

    public GetActiveBOMByPartAndTypeQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessBOMDto?> Handle(GetActiveBOMByPartAndTypeQuery request, CancellationToken cancellationToken)
    {
        var bom = await _context.ProcessBOMs
            .Include(b => b.Part)
            .Include(b => b.ProcessingType)
            .Include(b => b.BOMDetails)
            .Where(b => b.PartId == request.PartId 
                && b.ProcessingTypeId == request.ProcessingTypeId 
                && b.Status == "ACTIVE")
            .FirstOrDefaultAsync(cancellationToken);

        if (bom == null)
        {
            return null;
        }

        return new ProcessBOMDto
        {
            Id = bom.Id,
            PartId = bom.PartId,
            PartCode = bom.Part.Code,
            PartName = bom.Part.Name,
            ProcessingTypeId = bom.ProcessingTypeId,
            ProcessingTypeName = bom.ProcessingType.Name,
            Version = bom.Version,
            Status = bom.Status,
            EffectiveDate = bom.EffectiveDate,
            Name = bom.Name,
            Notes = bom.Notes,
            CreatedAt = bom.CreatedAt,
            CreatedBy = bom.CreatedBy,
            BOMDetails = bom.BOMDetails
                .OrderBy(d => d.SequenceOrder)
                .Select(d => new ProcessBOMDetailDto
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







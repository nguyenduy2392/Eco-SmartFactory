using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.ProcessBOM;

/// <summary>
/// Query to get Process BOM by ID
/// </summary>
public class GetProcessBOMByIdQuery : IRequest<ProcessBOMDto?>
{
    public Guid Id { get; set; }
}

public class GetProcessBOMByIdQueryHandler : IRequestHandler<GetProcessBOMByIdQuery, ProcessBOMDto?>
{
    private readonly ApplicationDbContext _context;

    public GetProcessBOMByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessBOMDto?> Handle(GetProcessBOMByIdQuery request, CancellationToken cancellationToken)
    {
        var bom = await _context.ProcessBOMs
            .Include(b => b.Part)
            .Include(b => b.ProcessingType)
            .Include(b => b.BOMDetails)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

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




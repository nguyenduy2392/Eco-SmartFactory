using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.ProcessBOM;

/// <summary>
/// Query to get all Process BOMs with optional filters
/// </summary>
public class GetAllProcessBOMQuery : IRequest<List<ProcessBOMListDto>>
{
    public Guid? PartId { get; set; }
    public Guid? ProcessingTypeId { get; set; }
    public string? ProcessingType { get; set; } // Code string (e.g., "EP_NHUA")
    public string? Status { get; set; }
}

public class ProcessBOMListDto
{
    public Guid Id { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string? PartName { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string Version { get; set; } = "V1";
    public string Status { get; set; } = "ACTIVE";
    public DateTime? EffectiveDate { get; set; }
    public int MaterialCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAllProcessBOMQueryHandler : IRequestHandler<GetAllProcessBOMQuery, List<ProcessBOMListDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProcessBOMQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcessBOMListDto>> Handle(GetAllProcessBOMQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProcessBOMs
            .Include(b => b.Part)
            .Include(b => b.ProcessingType)
            .Include(b => b.BOMDetails)
            .AsQueryable();

        // Apply filters
        if (request.PartId.HasValue && request.PartId.Value != Guid.Empty)
        {
            query = query.Where(b => b.PartId == request.PartId.Value);
        }

        if (request.ProcessingTypeId.HasValue && request.ProcessingTypeId.Value != Guid.Empty)
        {
            query = query.Where(b => b.ProcessingTypeId == request.ProcessingTypeId.Value);
        }
        else if (!string.IsNullOrEmpty(request.ProcessingType))
        {
            // Look up ProcessingType by code
            var processingType = await _context.ProcessingTypes
                .FirstOrDefaultAsync(pt => pt.Code == request.ProcessingType, cancellationToken);
            
            if (processingType != null)
            {
                query = query.Where(b => b.ProcessingTypeId == processingType.Id);
            }
            else
            {
                // If ProcessingType code not found, return empty list
                return new List<ProcessBOMListDto>();
            }
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(b => b.Status == request.Status);
        }

        var boms = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new ProcessBOMListDto
            {
                Id = b.Id,
                PartCode = b.Part.Code,
                PartName = b.Part.Name,
                ProcessingType = b.ProcessingType.Code,
                Version = b.Version,
                Status = b.Status,
                EffectiveDate = b.EffectiveDate,
                MaterialCount = b.BOMDetails.Count,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return boms;
    }
}


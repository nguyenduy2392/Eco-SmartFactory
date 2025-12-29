using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.ProcessingTypes;

public class GetAllProcessingTypesQuery : IRequest<List<ProcessingTypeDto>>
{
}

public class ProcessingTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAllProcessingTypesQueryHandler : IRequestHandler<GetAllProcessingTypesQuery, List<ProcessingTypeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProcessingTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcessingTypeDto>> Handle(GetAllProcessingTypesQuery request, CancellationToken cancellationToken)
    {
        var processingTypes = await _context.ProcessingTypes
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .Select(pt => new ProcessingTypeDto
            {
                Id = pt.Id,
                Code = pt.Code,
                Name = pt.Name,
                Description = pt.Description,
                DisplayOrder = pt.DisplayOrder,
                IsActive = pt.IsActive,
                CreatedAt = pt.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return processingTypes;
    }
}


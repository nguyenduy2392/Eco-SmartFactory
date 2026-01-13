using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.Queries.Parts;

namespace SmartFactory.Application.Queries.Parts;

public class GetAllPartsQuery : IRequest<List<PartDetailDto>>
{
}

public class GetAllPartsQueryHandler : IRequestHandler<GetAllPartsQuery, List<PartDetailDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllPartsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PartDetailDto>> Handle(GetAllPartsQuery request, CancellationToken cancellationToken)
    {
        var parts = await _context.Parts
            .Include(p => p.Product)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PartDetailDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                ProductId = p.ProductId,
                ProductName = p.Product.Name,
                ProductCode = p.Product.Code,
                Position = p.Position,
                Material = p.Material,
                Color = p.Color,
                Weight = p.Weight,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                Status = p.IsActive ? "In Production" : "Draft",
                Processes = new List<ProcessTypeDto>() // Empty list for list view
            })
            .ToListAsync(cancellationToken);

        return parts;
    }
}


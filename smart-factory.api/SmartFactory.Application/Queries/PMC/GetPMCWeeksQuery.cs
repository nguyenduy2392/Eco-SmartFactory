using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Queries.PMC;

/// <summary>
/// Query to get list of PMC weeks with pagination
/// </summary>
public class GetPMCWeeksQuery : IRequest<List<PMCWeekListItemDto>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool OnlyActive { get; set; } = false;
    public int? Take { get; set; }
}

public class GetPMCWeeksQueryHandler : IRequestHandler<GetPMCWeeksQuery, List<PMCWeekListItemDto>>
{
    private readonly ApplicationDbContext _context;

    public GetPMCWeeksQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PMCWeekListItemDto>> Handle(GetPMCWeeksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PMCWeeks
            .Include(w => w.Creator)
            .Include(w => w.Rows)
            .AsQueryable();
        
        if (request.FromDate.HasValue)
        {
            query = query.Where(w => w.WeekStartDate >= request.FromDate.Value);
        }
        
        if (request.ToDate.HasValue)
        {
            query = query.Where(w => w.WeekStartDate <= request.ToDate.Value);
        }
        
        if (request.OnlyActive)
        {
            query = query.Where(w => w.IsActive);
        }
        
        query = query.OrderByDescending(w => w.WeekStartDate)
                    .ThenByDescending(w => w.Version);
        
        if (request.Take.HasValue)
        {
            query = query.Take(request.Take.Value);
        }
        
        var weeks = await query.ToListAsync(cancellationToken);
        
        return weeks.Select(w => new PMCWeekListItemDto
        {
            Id = w.Id,
            WeekStartDate = w.WeekStartDate,
            WeekEndDate = w.WeekEndDate,
            Version = w.Version,
            WeekName = w.WeekName,
            IsActive = w.IsActive,
            Status = w.Status,
            CreatedByName = w.Creator?.FullName,
            CreatedAt = w.CreatedAt,
            TotalRows = w.Rows.Count
        }).ToList();
    }
}

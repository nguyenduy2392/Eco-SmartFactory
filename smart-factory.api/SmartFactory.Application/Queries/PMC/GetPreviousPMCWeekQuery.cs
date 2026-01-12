using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Queries.PMC;

/// <summary>
/// Query to get previous week's PMC
/// </summary>
public class GetPreviousPMCWeekQuery : IRequest<PMCWeekDto?>
{
    public DateTime WeekStartDate { get; set; }
}

public class GetPreviousPMCWeekQueryHandler : IRequestHandler<GetPreviousPMCWeekQuery, PMCWeekDto?>
{
    private readonly ApplicationDbContext _context;

    public GetPreviousPMCWeekQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PMCWeekDto?> Handle(GetPreviousPMCWeekQuery request, CancellationToken cancellationToken)
    {
        // Get the most recent active week before the given date
        var previousWeek = await _context.PMCWeeks
            .Include(w => w.Creator)
            .Include(w => w.Rows)
                .ThenInclude(r => r.Cells)
            .Where(w => w.WeekStartDate < request.WeekStartDate && w.IsActive)
            .OrderByDescending(w => w.WeekStartDate)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (previousWeek == null)
            return null;
        
        var weekDates = Enumerable.Range(0, 6)
            .Select(i => previousWeek.WeekStartDate.AddDays(i))
            .ToList();
        
        return new PMCWeekDto
        {
            Id = previousWeek.Id,
            WeekStartDate = previousWeek.WeekStartDate,
            WeekEndDate = previousWeek.WeekEndDate,
            Version = previousWeek.Version,
            WeekName = previousWeek.WeekName,
            IsActive = previousWeek.IsActive,
            Status = previousWeek.Status,
            Notes = previousWeek.Notes,
            CreatedBy = previousWeek.CreatedBy,
            CreatedByName = previousWeek.Creator?.FullName,
            CreatedAt = previousWeek.CreatedAt,
            UpdatedAt = previousWeek.UpdatedAt,
            WeekDates = weekDates,
            Rows = previousWeek.Rows.OrderBy(r => r.DisplayOrder).Select(r => new PMCRowDto
            {
                Id = r.Id,
                PMCWeekId = r.PMCWeekId,
                ProductCode = r.ProductCode,
                ComponentName = r.ComponentName,
                CustomerId = r.CustomerId,
                CustomerName = r.CustomerName,
                PlanType = r.PlanType,
                PlanTypeDisplay = PMCPlanTypes.GetDisplayName(r.PlanType),
                DisplayOrder = r.DisplayOrder,
                TotalValue = r.TotalValue,
                RowGroup = r.RowGroup,
                Notes = r.Notes,
                Cells = r.Cells.OrderBy(c => c.WorkDate).Select(c => new PMCCellDto
                {
                    Id = c.Id,
                    PMCRowId = c.PMCRowId,
                    WorkDate = c.WorkDate,
                    Value = c.Value,
                    IsEditable = c.IsEditable,
                    BackgroundColor = c.BackgroundColor,
                    Notes = c.Notes
                }).ToList()
            }).ToList()
        };
    }
}

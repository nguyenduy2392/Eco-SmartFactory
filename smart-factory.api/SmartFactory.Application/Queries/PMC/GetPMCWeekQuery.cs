using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Queries.PMC;

/// <summary>
/// Query to get PMC week by date or ID
/// </summary>
public class GetPMCWeekQuery : IRequest<PMCWeekDto?>
{
    public DateTime? WeekStartDate { get; set; }
    public Guid? PMCWeekId { get; set; }
}

public class GetPMCWeekQueryHandler : IRequestHandler<GetPMCWeekQuery, PMCWeekDto?>
{
    private readonly ApplicationDbContext _context;

    public GetPMCWeekQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PMCWeekDto?> Handle(GetPMCWeekQuery request, CancellationToken cancellationToken)
    {
        PMCWeek? week = null;
        
        if (request.PMCWeekId.HasValue)
        {
            week = await _context.PMCWeeks
                .Include(w => w.Creator)
                .Include(w => w.Rows)
                    .ThenInclude(r => r.Cells)
                .FirstOrDefaultAsync(w => w.Id == request.PMCWeekId.Value, cancellationToken);
        }
        else if (request.WeekStartDate.HasValue)
        {
            // Calculate Monday of the week containing the given date
            var givenDate = request.WeekStartDate.Value;
            var dayOfWeek = (int)givenDate.DayOfWeek;
            // If Sunday (0), go back 6 days to get Monday
            var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            var mondayOfWeek = givenDate.AddDays(-daysToSubtract);
            
            // Get PMC for the week containing this date
            week = await _context.PMCWeeks
                .Include(w => w.Creator)
                .Include(w => w.Rows)
                    .ThenInclude(r => r.Cells)
                .Where(w => w.WeekStartDate == mondayOfWeek && w.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            // Get current or next week's active PMC
            var today = DateTime.Today;
            var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            
            week = await _context.PMCWeeks
                .Include(w => w.Creator)
                .Include(w => w.Rows)
                    .ThenInclude(r => r.Cells)
                .Where(w => w.WeekStartDate >= monday && w.IsActive)
                .OrderBy(w => w.WeekStartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        if (week == null)
            return null;
        
        var weekDates = Enumerable.Range(0, 6)
            .Select(i => week.WeekStartDate.AddDays(i))
            .ToList();
        
        return new PMCWeekDto
        {
            Id = week.Id,
            WeekStartDate = week.WeekStartDate,
            WeekEndDate = week.WeekEndDate,
            Version = week.Version,
            WeekName = week.WeekName,
            IsActive = week.IsActive,
            Status = week.Status,
            Notes = week.Notes,
            CreatedBy = week.CreatedBy,
            CreatedByName = week.Creator?.FullName,
            CreatedAt = week.CreatedAt,
            UpdatedAt = week.UpdatedAt,
            WeekDates = weekDates,
            Rows = week.Rows.OrderBy(r => r.DisplayOrder).Select(r => new PMCRowDto
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

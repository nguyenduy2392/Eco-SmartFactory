using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.PMC;

/// <summary>
/// Command to save PMC week (updates existing data, no versioning)
/// </summary>
public class SavePMCWeekCommand : IRequest<PMCWeekDto>
{
    public Guid PMCWeekId { get; set; }
    public string? Notes { get; set; }
    public List<SavePMCRowRequest> Rows { get; set; } = new();
    public Guid CreatedBy { get; set; }
}

public class SavePMCWeekCommandHandler : IRequestHandler<SavePMCWeekCommand, PMCWeekDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SavePMCWeekCommandHandler> _logger;

    public SavePMCWeekCommandHandler(
        ApplicationDbContext context,
        ILogger<SavePMCWeekCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PMCWeekDto> Handle(SavePMCWeekCommand request, CancellationToken cancellationToken)
    {
        // Get current week with all related data
        var currentWeek = await _context.PMCWeeks
            .Include(w => w.Rows)
                .ThenInclude(r => r.Cells)
            .FirstOrDefaultAsync(w => w.Id == request.PMCWeekId, cancellationToken);
        
        if (currentWeek == null)
            throw new Exception("PMC Week not found");
        
        // Update week metadata
        currentWeek.Notes = request.Notes ?? currentWeek.Notes;
        currentWeek.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updating PMC Week {WeekId} - {WeekName}", 
            currentWeek.Id, currentWeek.WeekName);
        
        // Update rows and cells
        foreach (var rowRequest in request.Rows)
        {
            _logger.LogInformation("Processing row: ProductCode={ProductCode}, Component={Component}, PlanType={PlanType}, CustomerId={CustomerId}, Id={RowId}",
                rowRequest.ProductCode, rowRequest.ComponentName, rowRequest.PlanType, rowRequest.CustomerId, rowRequest.Id);
            
            // Find existing row by ID first, then by matching all key fields
            var existingRow = currentWeek.Rows.FirstOrDefault(r => r.Id == rowRequest.Id);
            
            if (existingRow == null && rowRequest.Id != null && rowRequest.Id != Guid.Empty)
            {
                _logger.LogWarning("Row ID {RowId} not found in current week, attempting to find by matching fields", rowRequest.Id);
            }
            
            // If not found by ID, try to find by matching fields
            if (existingRow == null)
            {
                existingRow = currentWeek.Rows.FirstOrDefault(r =>
                    r.ProductCode == rowRequest.ProductCode &&
                    r.ComponentName == rowRequest.ComponentName &&
                    r.PlanType == rowRequest.PlanType &&
                    r.CustomerId == rowRequest.CustomerId);
                
                if (existingRow != null)
                {
                    _logger.LogInformation("Found matching row by fields: {RowId}", existingRow.Id);
                }
            }
            
            if (existingRow != null)
            {
                _logger.LogInformation("Updating existing row {RowId} with TotalValue={TotalValue}", 
                    existingRow.Id, rowRequest.TotalValue);
                
                // Update existing row
                existingRow.TotalValue = rowRequest.TotalValue;
                existingRow.Notes = rowRequest.Notes;
                existingRow.UpdatedAt = DateTime.UtcNow;
                
                // Update cells
                foreach (var cellEntry in rowRequest.CellValues)
                {
                    if (DateTime.TryParse(cellEntry.Key, out var workDate))
                    {
                        var existingCell = existingRow.Cells.FirstOrDefault(c => c.WorkDate.Date == workDate.Date);
                        
                        if (existingCell != null)
                        {
                            // Update existing cell - ALWAYS update even if value is 0
                            _logger.LogInformation("Updating cell for row {RowId}, date {Date}: OldValue={OldValue}, NewValue={NewValue}", 
                                existingRow.Id, workDate.Date, existingCell.Value, cellEntry.Value);
                            existingCell.Value = cellEntry.Value;
                            existingCell.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // Create new cell - even if value is 0
                            _logger.LogInformation("Creating new cell for row {RowId}, date {Date}: Value={Value}", 
                                existingRow.Id, workDate.Date, cellEntry.Value);
                            existingRow.Cells.Add(new PMCCell
                            {
                                PMCRowId = existingRow.Id,
                                WorkDate = workDate,
                                Value = cellEntry.Value,
                                IsEditable = existingRow.PlanType != PMCPlanTypes.Requirement,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse date: {DateKey}", cellEntry.Key);
                    }
                }
            }
            else
            {
                _logger.LogInformation("Creating new row: ProductCode={ProductCode}, Component={Component}, PlanType={PlanType}",
                    rowRequest.ProductCode, rowRequest.ComponentName, rowRequest.PlanType);
                
                // Create new row
                var newRow = new PMCRow
                {
                    PMCWeekId = currentWeek.Id,
                    ProductCode = rowRequest.ProductCode,
                    ComponentName = rowRequest.ComponentName,
                    CustomerId = rowRequest.CustomerId,
                    PlanType = rowRequest.PlanType,
                    RowGroup = $"{rowRequest.ProductCode}_{rowRequest.ComponentName}",
                    DisplayOrder = currentWeek.Rows.Count > 0 ? currentWeek.Rows.Max(r => r.DisplayOrder) + 1 : 0,
                    TotalValue = rowRequest.TotalValue,
                    Notes = rowRequest.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Add cells
                foreach (var cellEntry in rowRequest.CellValues)
                {
                    if (DateTime.TryParse(cellEntry.Key, out var workDate))
                    {
                        newRow.Cells.Add(new PMCCell
                        {
                            PMCRowId = newRow.Id,
                            WorkDate = workDate,
                            Value = cellEntry.Value,
                            IsEditable = newRow.PlanType != PMCPlanTypes.Requirement,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                
                currentWeek.Rows.Add(newRow);
            }
        }
        
        _logger.LogInformation("About to save changes. Total rows in week: {RowCount}, Total cells across all rows: {CellCount}",
            currentWeek.Rows.Count, currentWeek.Rows.Sum(r => r.Cells.Count));
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully saved PMC Week {WeekId} with {RowCount} rows", 
                currentWeek.Id, currentWeek.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving PMC Week {WeekId}: {Message}", currentWeek.Id, ex.Message);
            throw;
        }
        
        // Return updated week
        return await GetPMCWeekDto(currentWeek.Id, cancellationToken);
    }
    
    private async Task<PMCWeekDto> GetPMCWeekDto(Guid pmcWeekId, CancellationToken cancellationToken)
    {
        var week = await _context.PMCWeeks
            .Include(w => w.Creator)
            .Include(w => w.Rows)
                .ThenInclude(r => r.Cells)
            .FirstOrDefaultAsync(w => w.Id == pmcWeekId, cancellationToken);
        
        if (week == null)
            throw new Exception("PMC Week not found");
        
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

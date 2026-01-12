using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.PMC;

/// <summary>
/// Command to create a new PMC week for planning
/// </summary>
public class CreatePMCWeekCommand : IRequest<PMCWeekDto>
{
    public DateTime? WeekStartDate { get; set; }
    public string? Notes { get; set; }
    public bool CopyFromPreviousWeek { get; set; } = false;
    public Guid CreatedBy { get; set; }
}

public class CreatePMCWeekCommandHandler : IRequestHandler<CreatePMCWeekCommand, PMCWeekDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreatePMCWeekCommandHandler> _logger;

    public CreatePMCWeekCommandHandler(
        ApplicationDbContext context,
        ILogger<CreatePMCWeekCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PMCWeekDto> Handle(CreatePMCWeekCommand request, CancellationToken cancellationToken)
    {
        // Calculate week dates (Monday to Saturday)
        var weekStart = request.WeekStartDate ?? GetCurrentOrNextWeekMonday();
        var weekEnd = weekStart.AddDays(5); // Saturday
        
        // Check if PMC week already exists for this week
        var existingWeek = await _context.PMCWeeks
            .Where(w => w.WeekStartDate == weekStart && w.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (existingWeek != null)
        {
            throw new Exception($"PMC already exists for week starting {weekStart:yyyy-MM-dd}. Use save to update it.");
        }
        
        // Get previous week for copy option
        var previousWeek = await _context.PMCWeeks
            .Include(w => w.Rows)
                .ThenInclude(r => r.Cells)
            .Where(w => w.WeekStartDate < weekStart && w.IsActive)
            .OrderByDescending(w => w.WeekStartDate)
            .FirstOrDefaultAsync(cancellationToken);
        
        // Create new PMC week (always version 1 since one week = one PMC)
        var pmcWeek = new PMCWeek
        {
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            Version = 1,
            WeekName = $"Week {GetWeekNumber(weekStart)} - {weekStart:MMM yyyy}",
            IsActive = true,
            Status = PMCStatus.Draft,
            Notes = request.Notes,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.PMCWeeks.Add(pmcWeek);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created PMC Week {WeekName} with ID: {WeekId}", 
            pmcWeek.WeekName, pmcWeek.Id);
        
        // Initialize rows from PO data or copy from previous week
        if (request.CopyFromPreviousWeek && previousWeek != null)
        {
            await CopyFromPreviousWeek(pmcWeek.Id, previousWeek.Id, cancellationToken);
        }
        else
        {
            await InitializeFromPurchaseOrders(pmcWeek, cancellationToken);
        }
        
        // Load and return full week data
        return await GetPMCWeekDto(pmcWeek.Id, cancellationToken);
    }
    
    private DateTime GetCurrentOrNextWeekMonday()
    {
        var today = DateTime.Today;
        var dayOfWeek = (int)today.DayOfWeek;
        
        // If today is Sunday (0), get next Monday
        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            return today.AddDays(1);
        }
        
        // If Monday to Saturday, get current week's Monday
        var daysToSubtract = dayOfWeek - 1; // Monday = 1
        return today.AddDays(-daysToSubtract);
    }
    
    private int GetWeekNumber(DateTime date)
    {
        var jan1 = new DateTime(date.Year, 1, 1);
        var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        var firstMonday = jan1.AddDays(daysOffset);
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        var firstWeek = cal.GetWeekOfYear(firstMonday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var targetWeek = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return targetWeek;
    }
    
    private async Task InitializeFromPurchaseOrders(PMCWeek pmcWeek, CancellationToken cancellationToken)
    {
        // Get all active POs - prioritize PHUN_IN but accept all if needed
        var pos = await _context.PurchaseOrders
            .Include(po => po.POProducts)
                .ThenInclude(pop => pop.Product)
                    .ThenInclude(p => p!.Parts)
            .Include(po => po.Customer)
            .Where(po => po.IsActive && 
                        (po.Status == "DRAFT" || po.Status == "APPROVED" || po.Status == "APPROVED_FOR_PMC"))
            .ToListAsync(cancellationToken);
        
        _logger.LogInformation("Found {POCount} POs for PMC initialization (all DRAFT/APPROVED)", pos.Count);
        
        // If we have PHUN/IN POs, use only those. Otherwise use all.
        var phunInPos = pos.Where(po => 
            po.ProcessingType == "PHUN_IN" || 
            po.ProcessingType == "PHUN" || 
            po.ProcessingType == "IN").ToList();
        
        var posToUse = phunInPos.Any() ? phunInPos : pos;
        
        _logger.LogInformation("Using {POCount} POs (PHUN/IN: {PhunInCount}, All: {AllCount})", 
            posToUse.Count, phunInPos.Count, pos.Count);
        
        var rows = new List<PMCRow>();
        var displayOrder = 0;
        
        foreach (var po in posToUse)
        {
            _logger.LogInformation("Processing PO {PONumber} with {ProductCount} products", 
                po.PONumber, po.POProducts.Count);
            
            foreach (var poProduct in po.POProducts)
            {
                if (poProduct.Product == null)
                {
                    _logger.LogWarning("POProduct has no Product linked");
                    continue;
                }
                
                var productCode = poProduct.Product.Code;
                
                // If product has parts, use them
                if (poProduct.Product.Parts != null && poProduct.Product.Parts.Any())
                {
                    _logger.LogInformation("Product {ProductCode} has {PartCount} parts", 
                        productCode, poProduct.Product.Parts.Count);
                    
                    foreach (var part in poProduct.Product.Parts)
                    {
                        var rowGroup = $"{productCode}_{part.Name}";
                        
                        // Create 3 rows for each component: Requirement, Production, Clamp
                        rows.Add(CreatePMCRow(pmcWeek.Id, productCode, part.Name, po.Customer?.Name, 
                            PMCPlanTypes.Requirement, rowGroup, displayOrder++, po.CustomerId));
                        
                        rows.Add(CreatePMCRow(pmcWeek.Id, productCode, part.Name, po.Customer?.Name, 
                            PMCPlanTypes.Production, rowGroup, displayOrder++, po.CustomerId));
                        
                        rows.Add(CreatePMCRow(pmcWeek.Id, productCode, part.Name, po.Customer?.Name, 
                            PMCPlanTypes.Clamp, rowGroup, displayOrder++, po.CustomerId));
                    }
                }
                else
                {
                    // If no parts defined, create rows for the product itself as a component
                    _logger.LogInformation("Product {ProductCode} has no parts, using product as component", 
                        productCode);
                    
                    var rowGroup = $"{productCode}_{poProduct.Product.Name}";
                    
                    rows.Add(CreatePMCRow(pmcWeek.Id, productCode, poProduct.Product.Name, po.Customer?.Name, 
                        PMCPlanTypes.Requirement, rowGroup, displayOrder++, po.CustomerId));
                    
                    rows.Add(CreatePMCRow(pmcWeek.Id, productCode, poProduct.Product.Name, po.Customer?.Name, 
                        PMCPlanTypes.Production, rowGroup, displayOrder++, po.CustomerId));
                    
                    rows.Add(CreatePMCRow(pmcWeek.Id, productCode, poProduct.Product.Name, po.Customer?.Name, 
                        PMCPlanTypes.Clamp, rowGroup, displayOrder++, po.CustomerId));
                }
            }
        }
        
        _logger.LogInformation("Created {RowCount} rows for PMC initialization", rows.Count);
        
        if (rows.Any())
        {
            _context.PMCRows.AddRange(rows);
            
            // Initialize cells with 0 values for all 6 days
            var cells = new List<PMCCell>();
            foreach (var row in rows)
            {
                for (int i = 0; i < 6; i++)
                {
                    var workDate = pmcWeek.WeekStartDate.AddDays(i);
                    cells.Add(new PMCCell
                    {
                        PMCRowId = row.Id,
                        WorkDate = workDate,
                        Value = 0,
                        IsEditable = row.PlanType != PMCPlanTypes.Requirement,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            
            _context.PMCCells.AddRange(cells);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Initialized {RowCount} rows with {CellCount} cells for PMC Week {WeekId}", 
                rows.Count, cells.Count, pmcWeek.Id);
        }
        else
        {
            _logger.LogWarning("No rows created for PMC Week {WeekId}. No POs found or POs have no Products.", 
                pmcWeek.Id);
        }
    }
    
    private PMCRow CreatePMCRow(Guid pmcWeekId, string productCode, string componentName, 
        string? customerName, string planType, string rowGroup, int displayOrder, Guid? customerId)
    {
        return new PMCRow
        {
            PMCWeekId = pmcWeekId,
            ProductCode = productCode,
            ComponentName = componentName,
            CustomerName = customerName,
            CustomerId = customerId,
            PlanType = planType,
            RowGroup = rowGroup,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private async Task CopyFromPreviousWeek(Guid newWeekId, Guid previousWeekId, CancellationToken cancellationToken)
    {
        var previousRows = await _context.PMCRows
            .Include(r => r.Cells)
            .Where(r => r.PMCWeekId == previousWeekId)
            .ToListAsync(cancellationToken);
        
        var newRows = new List<PMCRow>();
        var newCells = new List<PMCCell>();
        
        var newWeek = await _context.PMCWeeks.FindAsync(new object[] { newWeekId }, cancellationToken);
        
        foreach (var oldRow in previousRows)
        {
            var newRow = new PMCRow
            {
                PMCWeekId = newWeekId,
                ProductCode = oldRow.ProductCode,
                ComponentName = oldRow.ComponentName,
                CustomerName = oldRow.CustomerName,
                CustomerId = oldRow.CustomerId,
                PlanType = oldRow.PlanType,
                RowGroup = oldRow.RowGroup,
                DisplayOrder = oldRow.DisplayOrder,
                TotalValue = oldRow.TotalValue,
                Notes = oldRow.Notes,
                CreatedAt = DateTime.UtcNow
            };
            
            newRows.Add(newRow);
            
            // Copy cells with new week dates
            for (int i = 0; i < 6; i++)
            {
                var oldCell = oldRow.Cells.FirstOrDefault(c => c.WorkDate == oldRow.PMCWeek!.WeekStartDate.AddDays(i));
                var newWorkDate = newWeek!.WeekStartDate.AddDays(i);
                
                newCells.Add(new PMCCell
                {
                    PMCRowId = newRow.Id,
                    WorkDate = newWorkDate,
                    Value = oldCell?.Value ?? 0,
                    IsEditable = newRow.PlanType != PMCPlanTypes.Requirement,
                    BackgroundColor = oldCell?.BackgroundColor,
                    Notes = oldCell?.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        _context.PMCRows.AddRange(newRows);
        _context.PMCCells.AddRange(newCells);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Copied {RowCount} rows from previous week for PMC Week {WeekId}", 
            newRows.Count, newWeekId);
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

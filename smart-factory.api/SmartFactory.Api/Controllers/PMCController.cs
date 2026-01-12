using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.PMC;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.PMC;
using System.Security.Claims;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class PMCController : BaseApiController
{
    /// <summary>
    /// Get PMC week by ID or by date and version
    /// </summary>
    /// <param name="id">PMC Week ID (optional)</param>
    /// <param name="weekStart">Week start date in yyyy-MM-dd format (optional)</param>
    /// <param name="version">Version number (optional, defaults to latest active)</param>
    [HttpGet]
    public async Task<IActionResult> GetPMCWeek(
        [FromQuery] Guid? id, 
        [FromQuery] string? weekStart, 
        [FromQuery] int? version)
    {
        var query = new GetPMCWeekQuery();
        
        if (id.HasValue)
        {
            query.PMCWeekId = id.Value;
        }
        else if (!string.IsNullOrEmpty(weekStart))
        {
            if (DateTime.TryParse(weekStart, out var date))
            {
                query.WeekStartDate = date;
            }
            else
            {
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });
            }
        }
        
        var result = await Mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { error = "PMC Week not found" });
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Get list of PMC weeks
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetPMCWeeks(
        [FromQuery] string? fromDate,
        [FromQuery] string? toDate,
        [FromQuery] bool onlyActive = false,
        [FromQuery] int? take = null)
    {
        var query = new GetPMCWeeksQuery
        {
            OnlyActive = onlyActive,
            Take = take
        };
        
        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
        {
            query.FromDate = from;
        }
        
        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
        {
            query.ToDate = to;
        }
        
        var result = await Mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// Get previous week's PMC
    /// </summary>
    [HttpGet("previous")]
    public async Task<IActionResult> GetPreviousPMCWeek([FromQuery] string weekStart)
    {
        if (string.IsNullOrEmpty(weekStart) || !DateTime.TryParse(weekStart, out var date))
        {
            return BadRequest(new { error = "Invalid weekStart parameter. Use yyyy-MM-dd format" });
        }
        
        var query = new GetPreviousPMCWeekQuery { WeekStartDate = date };
        var result = await Mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { error = "No previous PMC Week found" });
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Create a new PMC week for planning
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreatePMCWeek([FromBody] CreatePMCWeekRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var command = new CreatePMCWeekCommand
            {
                WeekStartDate = request.WeekStartDate,
                Notes = request.Notes,
                CopyFromPreviousWeek = request.CopyFromPreviousWeek,
                CreatedBy = userId
            };
            
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Save PMC week (creates a new version)
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> SavePMCWeek([FromBody] SavePMCWeekRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var command = new SavePMCWeekCommand
            {
                PMCWeekId = request.PMCWeekId,
                Notes = request.Notes,
                Rows = request.Rows,
                CreatedBy = userId
            };
            
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Debug endpoint: Get available POs for PMC
    /// </summary>
    [HttpGet("debug/available-pos")]
    public async Task<IActionResult> GetAvailablePOs()
    {
        var query = new GetAvailablePOsForPMCQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }
    
    private new Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}

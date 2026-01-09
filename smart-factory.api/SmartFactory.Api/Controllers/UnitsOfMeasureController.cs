using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.Entities;

namespace SmartFactory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsOfMeasureController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitsOfMeasureController> _logger;

    public UnitsOfMeasureController(
        ApplicationDbContext context,
        ILogger<UnitsOfMeasureController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var units = await _context.UnitsOfMeasure
                .OrderBy(u => u.DisplayOrder)
                .ThenBy(u => u.Name)
                .ToListAsync();
            
            return Ok(units);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units of measure");
            return StatusCode(500, new { error = "Failed to retrieve units of measure" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var unit = await _context.UnitsOfMeasure.FindAsync(id);
            
            if (unit == null)
            {
                return NotFound();
            }
            
            return Ok(unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit of measure by ID");
            return StatusCode(500, new { error = "Failed to retrieve unit of measure" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UnitOfMeasureRequest request)
    {
        try
        {
            var existing = await _context.UnitsOfMeasure
                .FirstOrDefaultAsync(u => u.Code == request.Code);
            
            if (existing != null)
            {
                return BadRequest(new { error = $"Unit with code '{request.Code}' already exists" });
            }

            var unit = new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UnitsOfMeasure.Add(unit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit of measure");
            return StatusCode(500, new { error = "Failed to create unit of measure" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UnitOfMeasureRequest request)
    {
        try
        {
            var unit = await _context.UnitsOfMeasure.FindAsync(id);
            
            if (unit == null)
            {
                return NotFound();
            }

            var existing = await _context.UnitsOfMeasure
                .FirstOrDefaultAsync(u => u.Code == request.Code && u.Id != id);
            
            if (existing != null)
            {
                return BadRequest(new { error = $"Unit with code '{request.Code}' already exists" });
            }

            unit.Code = request.Code;
            unit.Name = request.Name;
            unit.Description = request.Description;
            unit.DisplayOrder = request.DisplayOrder;
            unit.IsActive = request.IsActive;
            unit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit of measure");
            return StatusCode(500, new { error = "Failed to update unit of measure" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var unit = await _context.UnitsOfMeasure.FindAsync(id);
            
            if (unit == null)
            {
                return NotFound();
            }

            _context.UnitsOfMeasure.Remove(unit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit of measure");
            return StatusCode(500, new { error = "Failed to delete unit of measure" });
        }
    }
}

public class UnitOfMeasureRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

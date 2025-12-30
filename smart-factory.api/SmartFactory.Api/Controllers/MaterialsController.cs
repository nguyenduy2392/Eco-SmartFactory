using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Materials;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.Materials;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class MaterialsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, [FromQuery] Guid? customerId)
    {
        var query = new GetAllMaterialsQuery 
        { 
            IsActive = isActive,
            CustomerId = customerId
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetMaterialByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Material with ID {id} not found" });
        }
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request)
    {
        try
        {
            var command = new CreateMaterialCommand
            {
                Code = request.Code,
                Name = request.Name,
                Type = request.Type,
                ColorCode = request.ColorCode,
                Supplier = request.Supplier,
                Unit = request.Unit,
                CurrentStock = request.CurrentStock,
                MinStock = request.MinStock,
                Description = request.Description,
                CustomerId = request.CustomerId
            };

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialRequest request)
    {
        try
        {
            var command = new UpdateMaterialCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Type = request.Type,
                ColorCode = request.ColorCode,
                Supplier = request.Supplier,
                Unit = request.Unit,
                CurrentStock = request.CurrentStock,
                MinStock = request.MinStock,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}


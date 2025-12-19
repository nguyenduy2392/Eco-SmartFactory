using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Products;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.Products;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProductsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery { ProductId = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}


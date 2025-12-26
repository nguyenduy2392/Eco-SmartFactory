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
    public async Task<IActionResult> GetAll([FromQuery] bool withPrices = false)
    {
        if (withPrices)
        {
            var queryWithPrices = new GetProductsWithPricesQuery();
            var result = await Mediator.Send(queryWithPrices);
            return Ok(result);
        }
        else
        {
            var query = new GetAllProductsQuery();
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }

    [HttpGet("{id}/detail", Name = "GetProductDetailByPO", Order = 1)]
    public async Task<IActionResult> GetDetailByPO(Guid id, [FromQuery] Guid purchaseOrderId)
    {
        if (purchaseOrderId == Guid.Empty)
        {
            return BadRequest(new { message = "purchaseOrderId is required" });
        }

        var query = new GetProductDetailByPOQuery 
        { 
            ProductId = id,
            PurchaseOrderId = purchaseOrderId
        };
        var result = await Mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { 
                message = "Product detail not found for the specified Product and Purchase Order",
                productId = id,
                purchaseOrderId = purchaseOrderId
            });
        }
        
        return Ok(result);
    }

    [HttpGet("{id}", Name = "GetProductById", Order = 2)]
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
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Category = request.Category
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}


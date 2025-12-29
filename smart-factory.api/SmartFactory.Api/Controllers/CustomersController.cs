using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Customers;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.Customers;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class CustomersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var query = new GetAllCustomersQuery { IsActive = isActive };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var command = new CreateCustomerCommand
        {
            Code = request.Code,
            Name = request.Name,
            Address = request.Address,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            PaymentTerms = request.PaymentTerms,
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}






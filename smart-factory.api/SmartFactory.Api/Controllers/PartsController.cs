using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Queries.Parts;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class PartsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllPartsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{partId}")]
    public async Task<IActionResult> GetById(Guid partId, [FromQuery] Guid purchaseOrderId)
    {
        var query = new GetPartByIdQuery 
        { 
            PartId = partId,
            PurchaseOrderId = purchaseOrderId
        };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}


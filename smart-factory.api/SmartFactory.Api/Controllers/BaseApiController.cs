using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SmartFactory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    protected IActionResult HandleResult<T>(T? result)
    {
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    protected IActionResult HandleResult<T>(T? result, string errorMessage)
    {
        if (result == null)
        {
            return BadRequest(new { message = errorMessage });
        }
        return Ok(result);
    }
}


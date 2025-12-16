using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Queries.Users;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetUserByIdQuery { UserId = userId.Value };
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }
}


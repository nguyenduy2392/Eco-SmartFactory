using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Auth;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Api.Controllers;

public class AuthController : BaseApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await Mediator.Send(command);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            FullName = request.FullName,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber
        };

        var result = await Mediator.Send(command);

        if (result == null)
        {
            return BadRequest(new { message = "Email already exists" });
        }

        return Ok(result);
    }
}


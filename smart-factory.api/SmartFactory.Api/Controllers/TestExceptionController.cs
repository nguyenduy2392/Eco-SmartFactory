using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Exceptions;

namespace SmartFactory.Api.Controllers;

/// <summary>
/// Test controller to demonstrate exception handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestExceptionController : BaseApiController
{
    /// <summary>
    /// Test NotFoundException
    /// </summary>
    [HttpGet("not-found")]
    public IActionResult TestNotFoundException()
    {
        throw new NotFoundException("Product", "ABC123");
    }

    /// <summary>
    /// Test ValidationException
    /// </summary>
    [HttpGet("validation-error")]
    public IActionResult TestValidationException()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email must be valid" } },
            { "Password", new[] { "Password must be at least 8 characters" } }
        };
        throw new ValidationException(errors);
    }

    /// <summary>
    /// Test BusinessException
    /// </summary>
    [HttpGet("business-error")]
    public IActionResult TestBusinessException()
    {
        throw new BusinessException("Cannot delete product because it has active orders", "PRODUCT_HAS_ORDERS");
    }

    /// <summary>
    /// Test UnauthorizedException
    /// </summary>
    [HttpGet("unauthorized")]
    public IActionResult TestUnauthorizedException()
    {
        throw new UnauthorizedException("Invalid credentials");
    }

    /// <summary>
    /// Test ForbiddenException
    /// </summary>
    [HttpGet("forbidden")]
    public IActionResult TestForbiddenException()
    {
        throw new ForbiddenException("You do not have permission to delete this resource");
    }

    /// <summary>
    /// Test unhandled exception
    /// </summary>
    [HttpGet("server-error")]
    public IActionResult TestServerError()
    {
        throw new Exception("This is an unhandled exception for testing");
    }

    /// <summary>
    /// Test ArgumentException
    /// </summary>
    [HttpGet("argument-error")]
    public IActionResult TestArgumentException()
    {
        throw new ArgumentException("Invalid argument provided");
    }

    /// <summary>
    /// Test InvalidOperationException
    /// </summary>
    [HttpGet("invalid-operation")]
    public IActionResult TestInvalidOperationException()
    {
        throw new InvalidOperationException("This operation is not allowed in the current state");
    }
}

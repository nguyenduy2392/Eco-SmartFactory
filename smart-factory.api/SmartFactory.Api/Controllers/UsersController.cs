using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Users;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.Users;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại
    /// </summary>
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

    /// <summary>
    /// Lấy danh sách tất cả người dùng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? keyword, [FromQuery] bool? isActive)
    {
        var query = new GetAllUsersQuery
        {
            Keyword = keyword,
            IsActive = isActive
        };

        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Tạo người dùng mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            FullName = request.FullName,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber
        };

        var result = await Mediator.Send(command);

        if (result == null)
        {
            return BadRequest(new { message = "Email đã tồn tại trong hệ thống" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            Id = id,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            IsActive = request.IsActive
        };

        var result = await Mediator.Send(command);

        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Xóa người dùng
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var command = new DeleteUserCommand { UserId = id };
        var result = await Mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        return Ok(new { message = "Xóa người dùng thành công" });
    }
}

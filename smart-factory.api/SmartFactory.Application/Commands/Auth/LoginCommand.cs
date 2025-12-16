using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Helpers;

namespace SmartFactory.Application.Commands.Auth;

public class LoginCommand : IRequest<LoginResponse?>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse?>
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ApplicationDbContext context,
        JwtHelper jwtHelper,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            return null;
        }

        var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.FullName);

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
}


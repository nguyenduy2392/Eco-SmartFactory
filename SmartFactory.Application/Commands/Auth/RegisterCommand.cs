using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;
using SmartFactory.Application.Helpers;

namespace SmartFactory.Application.Commands.Auth;

public class RegisterCommand : IRequest<LoginResponse?>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResponse?>
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        ApplicationDbContext context,
        JwtHelper jwtHelper,
        ILogger<RegisterCommandHandler> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<LoginResponse?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return null;
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered: {Email}", request.Email);

        // Generate token
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


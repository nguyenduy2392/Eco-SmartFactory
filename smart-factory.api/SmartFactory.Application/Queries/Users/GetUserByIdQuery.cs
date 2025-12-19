using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Users;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly ApplicationDbContext _context;

    public GetUserByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}


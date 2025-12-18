using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Users;

public class GetAllUsersQuery : IRequest<List<UserDto>>
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllUsersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        // Lọc theo từ khóa
        if (!string.IsNullOrEmpty(request.Keyword))
        {
            var keyword = request.Keyword.ToLower();
            query = query.Where(u => 
                u.FullName.ToLower().Contains(keyword) || 
                u.Email.ToLower().Contains(keyword) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
        }

        // Lọc theo trạng thái
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}


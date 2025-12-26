using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Customers;

public class GetAllCustomersQuery : IRequest<List<CustomerDto>>
{
    public bool? IsActive { get; set; }
}

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCustomersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Address = c.Address,
                ContactPerson = c.ContactPerson,
                Email = c.Email,
                Phone = c.Phone,
                PaymentTerms = c.PaymentTerms,
                Notes = c.Notes,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}




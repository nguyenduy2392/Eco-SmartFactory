using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

public class GetMaterialIssueByIdQuery : IRequest<MaterialIssueDto?>
{
    public Guid Id { get; set; }
}

public class GetMaterialIssueByIdQueryHandler : IRequestHandler<GetMaterialIssueByIdQuery, MaterialIssueDto?>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialIssueByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialIssueDto?> Handle(GetMaterialIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var issue = await _context.MaterialIssues
            .Include(mi => mi.Customer)
            .Include(mi => mi.Material)
            .Include(mi => mi.Warehouse)
            .FirstOrDefaultAsync(mi => mi.Id == request.Id, cancellationToken);

        if (issue == null)
        {
            return null;
        }

        return new MaterialIssueDto
        {
            Id = issue.Id,
            CustomerId = issue.CustomerId,
            CustomerName = issue.Customer.Name,
            MaterialId = issue.MaterialId,
            MaterialCode = issue.Material.Code,
            MaterialName = issue.Material.Name,
            WarehouseId = issue.WarehouseId,
            WarehouseCode = issue.Warehouse.Code,
            WarehouseName = issue.Warehouse.Name,
            BatchNumber = issue.BatchNumber,
            Quantity = issue.Quantity,
            Unit = issue.Unit,
            IssueDate = issue.IssueDate,
            Reason = issue.Reason,
            IssueNumber = issue.IssueNumber,
            Notes = issue.Notes,
            Status = issue.Status,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            CreatedBy = issue.CreatedBy,
            UpdatedBy = issue.UpdatedBy
        };
    }
}


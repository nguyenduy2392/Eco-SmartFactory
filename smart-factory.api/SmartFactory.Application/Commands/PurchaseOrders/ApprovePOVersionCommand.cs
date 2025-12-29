using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

/// <summary>
/// Command to approve PO version for PMC
/// PHASE 1 Rules:
/// - Only ONE version can be APPROVED_FOR_PMC
/// - Once approved, version becomes LOCKED
/// - Approving a new version automatically unapproves old versions
/// </summary>
public class ApprovePOVersionCommand : IRequest<PurchaseOrderDto>
{
    public Guid PurchaseOrderId { get; set; }
}

public class ApprovePOVersionCommandHandler : IRequestHandler<ApprovePOVersionCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApprovePOVersionCommandHandler> _logger;

    public ApprovePOVersionCommandHandler(
        ApplicationDbContext context,
        ILogger<ApprovePOVersionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(ApprovePOVersionCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

        if (po == null)
        {
            throw new Exception($"PO with ID {request.PurchaseOrderId} not found");
        }

        // Validate PO is in DRAFT status
        if (po.Status == "LOCKED")
        {
            throw new Exception($"PO {po.PONumber} {po.Version} is LOCKED and cannot be modified");
        }

        if (po.Status == "APPROVED_FOR_PMC")
        {
            throw new Exception($"PO {po.PONumber} {po.Version} is already APPROVED_FOR_PMC");
        }

        // PHASE 1: Find root PO and all versions
        var rootPOId = po.OriginalPOId ?? po.Id;
        var allVersions = await _context.PurchaseOrders
            .Where(p => p.Id == rootPOId || p.OriginalPOId == rootPOId)
            .ToListAsync(cancellationToken);

        // Unapprove and unlock any currently approved versions
        var currentlyApproved = allVersions.Where(p => p.Status == "APPROVED_FOR_PMC").ToList();
        foreach (var approvedPO in currentlyApproved)
        {
            approvedPO.Status = "DRAFT";
            approvedPO.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Unapproved PO {PONumber} {Version}", approvedPO.PONumber, approvedPO.Version);
        }

        // Approve this version
        po.Status = "APPROVED_FOR_PMC";
        po.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Approved PO {PONumber} {Version} for PMC", po.PONumber, po.Version);

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            CustomerId = po.CustomerId,
            CustomerName = po.Customer.Name,
            Version = po.Version,
            ProcessingType = po.ProcessingType,
            PODate = po.PODate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            OriginalPOId = po.OriginalPOId,
            VersionNumber = po.VersionNumber,
            IsActive = po.IsActive,
            CreatedAt = po.CreatedAt
        };
    }
}



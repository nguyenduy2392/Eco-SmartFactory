using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class UpdatePOMaterialStatusCommandHandler : IRequestHandler<UpdatePOMaterialStatusCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdatePOMaterialStatusCommandHandler> _logger;

    public UpdatePOMaterialStatusCommandHandler(
        ApplicationDbContext context,
        ILogger<UpdatePOMaterialStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(UpdatePOMaterialStatusCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

        if (po == null)
        {
            throw new Exception($"PO với ID {request.PurchaseOrderId} không tồn tại");
        }

        po.IsMaterialFullyReceived = request.IsMaterialFullyReceived;
        po.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated PO {PONumber} material status to {Status}", 
            po.PONumber, request.IsMaterialFullyReceived);

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            CustomerId = po.CustomerId,
            CustomerName = po.Customer.Name,
            Version = po.Version,
            Status = po.Status,
            ProcessingType = po.ProcessingType,
            PODate = po.PODate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            IsMaterialFullyReceived = po.IsMaterialFullyReceived,
            CreatedAt = po.CreatedAt,
            UpdatedAt = po.UpdatedAt
        };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.PurchaseOrders;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class UpdatePurchaseOrderGeneralInfoCommand : IRequest<PurchaseOrderDto>
{
    public Guid Id { get; set; }
    public string? PONumber { get; set; }
    public DateTime? PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseOrderGeneralInfoCommandHandler : IRequestHandler<UpdatePurchaseOrderGeneralInfoCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdatePurchaseOrderGeneralInfoCommandHandler> _logger;

    public UpdatePurchaseOrderGeneralInfoCommandHandler(
        ApplicationDbContext context,
        ILogger<UpdatePurchaseOrderGeneralInfoCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(UpdatePurchaseOrderGeneralInfoCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
        {
            throw new Exception($"Purchase Order with ID {request.Id} not found");
        }

        // Cho phép sửa mọi lúc - đã bỏ kiểm tra trạng thái DRAFT
        // if (po.Status != "DRAFT")
        // {
        //     throw new Exception("Chỉ có thể chỉnh sửa PO khi trạng thái là DRAFT");
        // }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.PONumber))
        {
            // Check if PONumber is unique (excluding current PO)
            var existingPO = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.PONumber == request.PONumber && p.Id != request.Id, cancellationToken);
            if (existingPO != null)
            {
                throw new Exception($"Mã PO '{request.PONumber}' đã tồn tại trong hệ thống");
            }
            po.PONumber = request.PONumber;
        }

        if (request.PODate.HasValue)
        {
            po.PODate = request.PODate.Value;
        }

        if (request.ExpectedDeliveryDate.HasValue)
        {
            po.ExpectedDeliveryDate = request.ExpectedDeliveryDate.Value;
        }

        if (request.Notes != null)
        {
            po.Notes = request.Notes;
        }

        po.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated PO general info: {PONumber} with ID: {POId}", po.PONumber, po.Id);

        // Return updated PO using query
        var query = new GetPurchaseOrderByIdQuery { Id = po.Id };
        var queryHandler = new GetPurchaseOrderByIdQueryHandler(_context);
        return await queryHandler.Handle(query, cancellationToken) 
            ?? throw new Exception("Failed to retrieve updated PO");
    }
}


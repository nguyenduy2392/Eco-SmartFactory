using MediatR;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

/// <summary>
/// Command để cập nhật trạng thái hoàn thành nhập NVL của PO
/// </summary>
public class UpdatePOMaterialStatusCommand : IRequest<PurchaseOrderDto>
{
    public Guid PurchaseOrderId { get; set; }
    public bool IsMaterialFullyReceived { get; set; }
}

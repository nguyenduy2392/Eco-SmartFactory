using MediatR;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.PurchaseOrders;

/// <summary>
/// Query để lấy lịch sử nhập kho của một PO
/// </summary>
public class GetPOMaterialReceiptHistoryQuery : IRequest<List<MaterialReceiptHistoryDto>>
{
    public Guid PurchaseOrderId { get; set; }
}

using MediatR;
using SmartFactory.Application.Services;

namespace SmartFactory.Application.Queries.PurchaseOrders;

/// <summary>
/// Query để lấy danh sách PO cho dropdown/search khi nhập kho
/// </summary>
public class GetPOsForSelectionQuery : IRequest<List<POForSelectionDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? CustomerId { get; set; }
}

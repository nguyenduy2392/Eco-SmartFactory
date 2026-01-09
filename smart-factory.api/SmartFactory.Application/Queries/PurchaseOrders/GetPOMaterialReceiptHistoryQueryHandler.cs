using MediatR;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Services;

namespace SmartFactory.Application.Queries.PurchaseOrders;

public class GetPOMaterialReceiptHistoryQueryHandler : IRequestHandler<GetPOMaterialReceiptHistoryQuery, List<MaterialReceiptHistoryDto>>
{
    private readonly StockInService _stockInService;

    public GetPOMaterialReceiptHistoryQueryHandler(StockInService stockInService)
    {
        _stockInService = stockInService;
    }

    public async Task<List<MaterialReceiptHistoryDto>> Handle(GetPOMaterialReceiptHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _stockInService.GetPOReceiptHistoryAsync(request.PurchaseOrderId);
    }
}

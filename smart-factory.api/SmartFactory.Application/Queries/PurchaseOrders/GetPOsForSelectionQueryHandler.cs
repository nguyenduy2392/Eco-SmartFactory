using MediatR;
using SmartFactory.Application.Services;

namespace SmartFactory.Application.Queries.PurchaseOrders;

public class GetPOsForSelectionQueryHandler : IRequestHandler<GetPOsForSelectionQuery, List<POForSelectionDto>>
{
    private readonly StockInService _stockInService;

    public GetPOsForSelectionQueryHandler(StockInService stockInService)
    {
        _stockInService = stockInService;
    }

    public async Task<List<POForSelectionDto>> Handle(GetPOsForSelectionQuery request, CancellationToken cancellationToken)
    {
        return await _stockInService.GetPOsForSelectionAsync(request.SearchTerm, request.CustomerId);
    }
}

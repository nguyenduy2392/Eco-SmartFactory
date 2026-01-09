using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Services;
using System.Security.Claims;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class StockInController : BaseApiController
{
    private readonly StockInService _stockInService;

    public StockInController(StockInService stockInService)
    {
        _stockInService = stockInService;
    }

    /// <summary>
    /// Nhập kho nguyên vật liệu (có thể gắn hoặc không gắn PO)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StockIn([FromBody] StockInRequest request)
    {
        try
        {
            var currentUser = User.FindFirst(ClaimTypes.Name)?.Value 
                           ?? User.FindFirst(ClaimTypes.Email)?.Value 
                           ?? "System";

            var result = await _stockInService.ProcessStockInAsync(request, currentUser);

            if (!result.Success)
            {
                return BadRequest(new 
                { 
                    error = result.ErrorMessage, 
                    errors = result.Errors 
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                receiptIds = result.CreatedReceiptIds,
                historyIds = result.CreatedHistoryIds,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

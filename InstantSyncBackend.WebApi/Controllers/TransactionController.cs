using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstantSyncBackend.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TransactionController(ITransactionService _transactionService) : ControllerBase
{
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferFunds([FromBody] TransferDto transferDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _transactionService.TransferFundsAsync(userId, transferDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    [HttpPost("add-funds")]
    public async Task<IActionResult> AddFunds([FromBody] AddFundsDto addFundsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _transactionService.AddFundsAsync(userId, addFundsDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _transactionService.GetTransactionHistoryAsync(userId, startDate, endDate);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    [HttpGet("status/{transactionReference}")]
    public async Task<IActionResult> GetTransactionStatus(string transactionReference)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _transactionService.GetTransactionStatusAsync(userId, transactionReference);
        return StatusCode(result.StatusCode ?? 200, result);
    }
}
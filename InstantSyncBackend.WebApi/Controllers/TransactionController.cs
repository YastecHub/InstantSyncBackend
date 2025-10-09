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
    /// <summary>
    /// Transfers funds to another account
    /// </summary>
    /// <param name="transferDto">Transfer details including beneficiary account and amount</param>
    /// <returns>Transaction response with reference number</returns>
    [HttpPost("send-money")]
    public async Task<IActionResult> SendMoney([FromBody] TransferDto transferDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated");
        }

        var result = await _transactionService.TransferFundsAsync(userId, transferDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Adds funds to user's account
    /// </summary>
    /// <param name="addFundsDto">Deposit details including amount and payment method</param>
    /// <returns>Transaction response with reference number</returns>
    [HttpPost("deposit")]
    public async Task<IActionResult> DepositFunds([FromBody] AddFundsDto addFundsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated");
        }

        var result = await _transactionService.AddFundsAsync(userId, addFundsDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Gets user's transaction history
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of user's transactions</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated");
        }

        var result = await _transactionService.GetTransactionHistoryAsync(userId, startDate, endDate);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Gets the status of a specific transaction
    /// </summary>
    /// <param name="transactionReference">Transaction reference number</param>
    /// <returns>Transaction status and details</returns>
    [HttpGet("status/{transactionReference}")]
    public async Task<IActionResult> CheckTransactionStatus(string transactionReference)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated");
        }

        if (string.IsNullOrWhiteSpace(transactionReference))
        {
            return BadRequest("Transaction reference is required");
        }

        var result = await _transactionService.GetTransactionStatusAsync(userId, transactionReference);
        return StatusCode(result.StatusCode ?? 200, result);
    }
}
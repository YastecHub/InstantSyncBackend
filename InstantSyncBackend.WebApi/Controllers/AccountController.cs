using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstantSyncBackend.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Gets account details for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get account details for</param>
    /// <returns>Account details including balance and account number</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<BaseResponse<AccountDetailsDto>>> GetUserAccount(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(BaseResponse<AccountDetailsDto>.Failure("User ID is required"));
        }

        var response = await _accountService.GetAccountDetailsByUserIdAsync(userId);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's account details
    /// </summary>
    /// <returns>Current user's account details</returns>
    [HttpGet("my-account")]
    public async Task<ActionResult<BaseResponse<AccountDetailsDto>>> GetMyAccount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(BaseResponse<AccountDetailsDto>.Failure("User not authenticated"));
        }

        var response = await _accountService.GetAccountDetailsByUserIdAsync(userId);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets account balance for the current authenticated user
    /// </summary>
    /// <returns>Current user's account balance information</returns>
    [HttpGet("balance")]
    public async Task<ActionResult<BaseResponse<AccountDetailsDto>>> GetAccountBalance()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(BaseResponse<AccountDetailsDto>.Failure("User not authenticated"));
        }

        var response = await _accountService.GetAccountDetailsByUserIdAsync(userId);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
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

    [HttpGet("details/{userId}")]
    public async Task<ActionResult<BaseResponse<AccountDetailsDto>>> GetAccountDetails(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(BaseResponse<AccountDetailsDto>.Failure("UserId is required"));
        }

        var response = await _accountService.GetAccountDetailsByUserIdAsync(userId);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<BaseResponse<AccountDetailsDto>>> GetMyAccountDetails()
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
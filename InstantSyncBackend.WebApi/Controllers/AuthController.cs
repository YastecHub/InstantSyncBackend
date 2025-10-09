using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace InstantSyncBackend.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService _authService) : ControllerBase
{
    /// <summary>
    /// Creates a new user account with associated banking account
    /// </summary>
    /// <param name="registerDto">User registration details</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("create-account")]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Authenticates user and returns JWT token
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Initiates password reset process
    /// </summary>
    /// <param name="forgotPasswordDto">Email for password reset</param>
    /// <returns>Success message</returns>
    [HttpPost("reset-password-request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }

    /// <summary>
    /// Resets user password using reset token
    /// </summary>
    /// <param name="resetPasswordDto">Reset password details</param>
    /// <returns>Success message</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        return StatusCode(result.StatusCode ?? 200, result);
    }
}

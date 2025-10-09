using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IServices;
using InstantSyncBackend.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Net;
using InstantSyncBackend.Application.Interfaces.IRepositories;

namespace InstantSyncBackend.Persistence.Services;

public class AuthService(UserManager<ApplicationUser> _userManager, IJwtTokenGenerator _jwtTokenGenerator, IValidator<RegisterDto> _registerValidator, IValidator<LoginDto> _loginValidator, IValidator<ForgotPasswordDto> _forgotPasswordValidator, IEmailService _emailService, IAccountRepository _accountRepository, ILogger<AuthService> _logger) : IAuthService
{
    public async Task<BaseResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        _logger.LogInformation("Starting user registration process for email: {Email}", registerDto.Email);
        
        // Validate input data
        var validationResult = await _registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Registration validation failed for email: {Email}", registerDto.Email);
            return BaseResponse<AuthResponseDto>.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User already exists with email: {Email}", registerDto.Email);
            return BaseResponse<AuthResponseDto>.Failure("User with this email already exists");
        }

        // Generate unique account number
        string accountNumber = await GenerateUniqueAccountNumberAsync();
        
        // Create user entity
        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            FullName = registerDto.FullName,
            AccountNumber = accountNumber,
            EmailConfirmed = true
        };

        // Create user in Identity system
        var userResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!userResult.Succeeded)
        {
            _logger.LogError("User creation failed for email: {Email}. Errors: {Errors}", 
                registerDto.Email, string.Join(", ", userResult.Errors.Select(e => e.Description)));
            return BaseResponse<AuthResponseDto>.ValidationFailure(userResult.Errors.Select(e => e.Description).ToList());
        }

        // Create associated account with zero balances
        try
        {
            var account = new Account
            {
                UserId = user.Id,
                AccountNumber = accountNumber,
                Balance = 0.00m,         
                PendingBalance = 0.00m,
                User = user
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();
            
            _logger.LogInformation("Account successfully created for user {UserId} with account number {AccountNumber}", 
                user.Id, accountNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for user {UserId}. Rolling back user creation.", user.Id);
            
            // Rollback user creation if account creation fails
            await _userManager.DeleteAsync(user);
            
            return BaseResponse<AuthResponseDto>.Failure("Failed to create user account. Please try again.");
        }

        // Generate JWT token for immediate login
        var token = _jwtTokenGenerator.GenerateToken(user);
        
        _logger.LogInformation("User and account successfully created for email: {Email} with account number: {AccountNumber}", 
            registerDto.Email, accountNumber);

        var response = new AuthResponseDto
        { 
            Token = token,
            Message = "Account created successfully",
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };
        
        return BaseResponse<AuthResponseDto>.Succes(response, "Registration successful");
    }

    public async Task<BaseResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Processing login attempt for: {EmailOrPhone}", loginDto.EmailOrPhone);
        
        var validationResult = await _loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Login validation failed for: {EmailOrPhone}", loginDto.EmailOrPhone);
            return BaseResponse<AuthResponseDto>.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await _userManager.FindByEmailAsync(loginDto.EmailOrPhone) ??
                   await _userManager.FindByNameAsync(loginDto.EmailOrPhone) ??
                   (await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.EmailOrPhone));

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogWarning("Invalid login attempt for: {EmailOrPhone}", loginDto.EmailOrPhone);
            return BaseResponse<AuthResponseDto>.Unauthorized("Invalid credentials");
        }

        _logger.LogInformation("User successfully logged in: {EmailOrPhone}", loginDto.EmailOrPhone);
        var token = _jwtTokenGenerator.GenerateToken(user);
        var response = new AuthResponseDto
        { 
            Token = token,
            Message = "Login successful",
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };
        return BaseResponse<AuthResponseDto>.Succes(response);
    }

    public async Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        _logger.LogInformation("Processing forgot password request for email: {Email}", forgotPasswordDto.Email);
        
        var validationResult = await _forgotPasswordValidator.ValidateAsync(forgotPasswordDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Forgot password validation failed for email: {Email}", forgotPasswordDto.Email);
            return BaseResponse<string>.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for forgot password request: {Email}", forgotPasswordDto.Email);
            return BaseResponse<string>.Failure("If the email exists in our system, you will receive a password reset link.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = $"https://yourfrontend.com/reset-password?email={WebUtility.UrlEncode(user.Email)}&token={encodedToken}";
        
        var emailBody = $@"
            <h2>Reset Your Password</h2>
            <p>Hello {user.FullName},</p>
            <p>You have requested to reset your password. Please click the link below to set a new password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you didn't request this, you can safely ignore this email.</p>
            <p>This link will expire in 24 hours.</p>
            <p>Best regards,<br>InstantSync Team</p>";

        var emailSent = await _emailService.SendEmailAsync(
            user.Email,
            "Reset Your Password - InstantSync",
            emailBody);

        if (!emailSent)
        {
            _logger.LogError("Failed to send password reset email to: {Email}", forgotPasswordDto.Email);
            return BaseResponse<string>.Failure("Failed to send password reset email. Please try again later.");
        }

        _logger.LogInformation("Password reset email sent successfully to: {Email}", forgotPasswordDto.Email);
        return BaseResponse<string>.Succes(
            "If the email exists in our system, you will receive a password reset link.",
            "Reset link sent");
    }

    public async Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return BaseResponse<string>.Failure("Invalid request");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            return BaseResponse<string>.ValidationFailure(result.Errors.Select(e => e.Description).ToList());
        }

        await _emailService.SendEmailAsync(
            user.Email,
            "Password Reset Successful - InstantSync",
            $@"<h2>Password Reset Successful</h2>
               <p>Hello {user.FullName},</p>
               <p>Your password has been successfully reset.</p>
               <p>If you did not perform this action, please contact our support team immediately.</p>
               <p>Best regards,<br>InstantSync Team</p>");

        return BaseResponse<string>.Succes("Your password has been reset successfully");
    }

    /// <summary>
    /// Generates a unique 10-digit account number
    /// </summary>
    private async Task<string> GenerateUniqueAccountNumberAsync()
    {
        string accountNumber;
        var random = new Random();
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            // Generate 10-digit account number
            accountNumber = string.Concat(Enumerable.Range(0, 10).Select(_ => random.Next(0, 10).ToString()));
            attempts++;
            
            if (attempts > maxAttempts)
            {
                // Fallback with timestamp to ensure uniqueness
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                accountNumber = timestamp.Substring(Math.Max(0, timestamp.Length - 10)).PadLeft(10, '0');
                break;
            }
        }
        while (await _userManager.Users.AnyAsync(u => u.AccountNumber == accountNumber) || 
               await _accountRepository.AccountNumberExistsAsync(accountNumber));

        return accountNumber;
    }
}

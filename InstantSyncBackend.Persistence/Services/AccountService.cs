using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace InstantSyncBackend.Persistence.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<AccountDetailsDto>> GetAccountDetailsByUserIdAsync(string userId)
    {
        _logger.LogInformation("Retrieving account details for user: {UserId}", userId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetAccountDetailsByUserIdAsync called with null or empty userId");
                return BaseResponse<AccountDetailsDto>.Failure("User ID cannot be null or empty");
            }

            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                _logger.LogWarning("Account not found for user: {UserId}", userId);
                return BaseResponse<AccountDetailsDto>.Failure("Account not found for this user");
            }

            var accountDetails = new AccountDetailsDto
            {
                UserId = account.UserId,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                PendingBalance = account.PendingBalance
            };

            _logger.LogInformation("Successfully retrieved account details for user: {UserId}, Account: {AccountNumber}", 
                userId, account.AccountNumber);

            return BaseResponse<AccountDetailsDto>.Succes(accountDetails, "Account details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account details for user {UserId}", userId);
            return BaseResponse<AccountDetailsDto>.Failure("An error occurred while retrieving account details");
        }
    }
}
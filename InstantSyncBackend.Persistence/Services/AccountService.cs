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
        try
        {
            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                return BaseResponse<AccountDetailsDto>.Failure("Account not found");
            }

            var accountDetails = new AccountDetailsDto
            {
                UserId = account.UserId,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                PendingBalance = account.PendingBalance
            };

            return BaseResponse<AccountDetailsDto>.Succes(accountDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account details for user {UserId}", userId);
            return BaseResponse<AccountDetailsDto>.Failure("An error occurred while retrieving account details");
        }
    }
}
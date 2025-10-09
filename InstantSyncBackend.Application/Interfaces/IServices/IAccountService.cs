using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;

namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface IAccountService
{
    Task<BaseResponse<AccountDetailsDto>> GetAccountDetailsByUserIdAsync(string userId);
    Task<BaseResponse<AccountDetailsDto>> GetAccountDetailsByAccountNumberAsync(string accountNumber);
}
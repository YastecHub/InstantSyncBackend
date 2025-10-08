using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;

namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface ITransactionService
{
    Task<BaseResponse<TransactionResponseDto>> TransferFundsAsync(string userId, TransferDto transferDto);
    Task<BaseResponse<TransactionResponseDto>> AddFundsAsync(string userId, AddFundsDto addFundsDto);
    Task<BaseResponse<List<TransactionHistoryDto>>> GetTransactionHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<BaseResponse<TransactionHistoryDto>> GetTransactionStatusAsync(string userId, string transactionReference);
}
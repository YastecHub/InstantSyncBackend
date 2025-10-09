using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Domain.Enums;

namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface ITransactionQueueService
{
    Task<BaseResponse<string>> EnqueueTransactionAsync(Guid transactionId, TransactionStatus targetStatus, int priority = 1);
    Task<BaseResponse<string>> ProcessNextBatchAsync();
    Task<BaseResponse<string>> ProcessTransactionStateAsync(Guid transactionId);
    Task<BaseResponse<List<TransactionQueue>>> GetQueueStatusAsync();
    Task<BaseResponse<string>> RetryFailedTransactionAsync(Guid transactionId);
    Task<BaseResponse<int>> GetQueueCountAsync();
}
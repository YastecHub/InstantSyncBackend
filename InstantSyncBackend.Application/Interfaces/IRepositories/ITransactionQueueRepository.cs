using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Domain.Enums;

namespace InstantSyncBackend.Application.Interfaces.IRepositories;

public interface ITransactionQueueRepository
{
    Task<TransactionQueue?> GetByIdAsync(Guid id);
    Task<TransactionQueue?> GetByTransactionIdAsync(Guid transactionId);
    Task<IEnumerable<TransactionQueue>> GetPendingItemsAsync(int batchSize = 10);
    Task<IEnumerable<TransactionQueue>> GetRetryableItemsAsync();
    Task<IEnumerable<TransactionQueue>> GetByStatusAsync(TransactionStatus status);
    Task AddAsync(TransactionQueue entity);
    Task UpdateAsync(TransactionQueue entity);
    Task DeleteAsync(TransactionQueue entity);
    Task<int> GetQueueCountAsync();
    Task<IEnumerable<TransactionQueue>> GetStuckTransactionsAsync(TimeSpan timeout);
    Task SaveChangesAsync();
}
using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Domain.Enums;
using InstantSyncBackend.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace InstantSyncBackend.Persistence.Repositories;

public class TransactionQueueRepository(ApplicationDbContext _dbContext) : ITransactionQueueRepository
{
    public async Task<TransactionQueue?> GetByIdAsync(Guid id)
    {
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .FirstOrDefaultAsync(tq => tq.Id == id);
    }

    public async Task<TransactionQueue?> GetByTransactionIdAsync(Guid transactionId)
    {
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .FirstOrDefaultAsync(tq => tq.TransactionId == transactionId);
    }

    public async Task<IEnumerable<TransactionQueue>> GetPendingItemsAsync(int batchSize = 10)
    {
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .Where(tq => !tq.IsProcessing && 
                        tq.ProcessedAt == null && 
                        tq.ScheduledProcessTime <= DateTime.UtcNow &&
                        tq.RetryCount < tq.MaxRetries)
            .OrderBy(tq => tq.Priority)
            .ThenBy(tq => tq.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionQueue>> GetRetryableItemsAsync()
    {
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .Where(tq => tq.RetryCount < tq.MaxRetries && 
                        tq.ProcessedAt == null &&
                        !tq.IsProcessing)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionQueue>> GetByStatusAsync(TransactionStatus status)
    {
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .Where(tq => tq.CurrentStatus == status)
            .ToListAsync();
    }

    public async Task<int> GetQueueCountAsync()
    {
        return await _dbContext.TransactionQueues
            .CountAsync(tq => tq.ProcessedAt == null);
    }

    public async Task<IEnumerable<TransactionQueue>> GetStuckTransactionsAsync(TimeSpan timeout)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeout);
        return await _dbContext.TransactionQueues
            .Include(tq => tq.Transaction)
            .Where(tq => tq.IsProcessing && tq.UpdatedAt < cutoffTime)
            .ToListAsync();
    }

    public async Task AddAsync(TransactionQueue entity)
    {
        await _dbContext.TransactionQueues.AddAsync(entity);
    }

    public async Task UpdateAsync(TransactionQueue entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.TransactionQueues.Update(entity);
    }

    public async Task DeleteAsync(TransactionQueue entity)
    {
        _dbContext.TransactionQueues.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
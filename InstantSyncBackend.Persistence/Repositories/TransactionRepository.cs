using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace InstantSyncBackend.Persistence.Repositories;

public class TransactionRepository(ApplicationDbContext _dbContext) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Transactions.FindAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbContext.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transaction?> GetUserTransactionByReferenceAsync(string userId, string transactionReference)
    {
        return await _dbContext.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Account.UserId == userId && t.TransactionReference == transactionReference);
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _dbContext.Transactions.ToListAsync();
    }

    public async Task AddAsync(Transaction entity)
    {
        await _dbContext.Transactions.AddAsync(entity);
    }

    public async Task UpdateAsync(Transaction entity)
    {
        _dbContext.Transactions.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Transaction entity)
    {
        _dbContext.Transactions.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
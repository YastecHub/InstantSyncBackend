using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace InstantSyncBackend.Persistence.Repositories;

public class AccountRepository(ApplicationDbContext _dbContext) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber)
    {
        return await _dbContext.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _dbContext.Accounts.ToListAsync();
    }

    public async Task AddAsync(Account entity)
    {
        await _dbContext.Accounts.AddAsync(entity);
    }

    public async Task UpdateAsync(Account entity)
    {
        _dbContext.Accounts.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Account entity)
    {
        _dbContext.Accounts.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
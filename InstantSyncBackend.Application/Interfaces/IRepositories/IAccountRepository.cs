using InstantSyncBackend.Domain.Entities;

namespace InstantSyncBackend.Application.Interfaces.IRepositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByUserIdAsync(string userId);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
    Task<List<Account>> GetAllAsync();
    Task AddAsync(Account entity);
    Task UpdateAsync(Account entity);
    Task DeleteAsync(Account entity);
    Task SaveChangesAsync();
}
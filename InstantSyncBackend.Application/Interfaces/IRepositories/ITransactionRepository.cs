using InstantSyncBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstantSyncBackend.Application.Interfaces.IRepositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Transaction?> GetUserTransactionByReferenceAsync(string userId, string transactionReference);
    Task<List<Transaction>> GetAllAsync();
    Task AddAsync(Transaction entity);
    Task UpdateAsync(Transaction entity);
    Task DeleteAsync(Transaction entity);
    Task SaveChangesAsync();
}
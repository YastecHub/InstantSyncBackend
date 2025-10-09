using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Application.Interfaces.IServices;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InstantSyncBackend.Persistence.Services;

public class TransactionQueueService(
    ITransactionQueueRepository _queueRepository,
    ITransactionRepository _transactionRepository,
    IAccountRepository _accountRepository,
    ILogger<TransactionQueueService> _logger) : ITransactionQueueService
{
    public async Task<BaseResponse<string>> EnqueueTransactionAsync(Guid transactionId, TransactionStatus targetStatus, int priority = 1)
    {
        try
        {
            _logger.LogInformation("Enqueueing transaction {TransactionId} for status {TargetStatus}", transactionId, targetStatus);

            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return BaseResponse<string>.Failure("Transaction not found");
            }

            // Check if already queued
            var existingQueue = await _queueRepository.GetByTransactionIdAsync(transactionId);
            if (existingQueue != null && existingQueue.ProcessedAt == null)
            {
                return BaseResponse<string>.Failure("Transaction already in queue");
            }

            var queueItem = new TransactionQueue
            {
                TransactionId = transactionId,
                CurrentStatus = transaction.Status,
                NextStatus = targetStatus,
                ScheduledProcessTime = CalculateProcessingTime(targetStatus),
                Priority = priority,
                QueueType = DetermineQueueType(transaction.Amount)
            };

            await _queueRepository.AddAsync(queueItem);
            await _queueRepository.SaveChangesAsync();

            _logger.LogInformation("Transaction {TransactionId} successfully enqueued", transactionId);
            return BaseResponse<string>.Succes($"Transaction enqueued successfully. Queue ID: {queueItem.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing transaction {TransactionId}", transactionId);
            return BaseResponse<string>.Failure("Failed to enqueue transaction");
        }
    }

    public async Task<BaseResponse<string>> ProcessNextBatchAsync()
    {
        try
        {
            var pendingItems = await _queueRepository.GetPendingItemsAsync(10);
            
            if (!pendingItems.Any())
            {
                return BaseResponse<string>.Succes("No items to process");
            }

            var processedCount = 0;
            foreach (var queueItem in pendingItems)
            {
                var result = await ProcessTransactionStateAsync(queueItem.TransactionId);
                if (result.Success)
                {
                    processedCount++;
                }
            }

            return BaseResponse<string>.Succes($"Processed {processedCount} transactions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction batch");
            return BaseResponse<string>.Failure("Failed to process batch");
        }
    }

    public async Task<BaseResponse<string>> ProcessTransactionStateAsync(Guid transactionId)
    {
        try
        {
            var queueItem = await _queueRepository.GetByTransactionIdAsync(transactionId);
            if (queueItem == null)
            {
                return BaseResponse<string>.Failure("Transaction not found in queue");
            }

            // Mark as processing
            queueItem.IsProcessing = true;
            queueItem.ProcessingNode = Environment.MachineName;
            await _queueRepository.UpdateAsync(queueItem);
            await _queueRepository.SaveChangesAsync();

            var transaction = queueItem.Transaction;
            var account = await _accountRepository.GetByIdAsync(transaction.AccountId);

            if (account == null)
            {
                return BaseResponse<string>.Failure("Account not found");
            }

            // Process based on target status
            var success = await ProcessStatusTransition(transaction, account, queueItem.NextStatus);

            // Update queue item
            queueItem.ProcessedAt = DateTime.UtcNow;
            queueItem.IsProcessing = false;

            if (!success)
            {
                queueItem.RetryCount++;
                queueItem.ErrorMessage = "Status transition failed";
                queueItem.ScheduledProcessTime = CalculateRetryTime(queueItem.RetryCount);
                
                if (queueItem.RetryCount >= queueItem.MaxRetries)
                {
                    // Mark transaction as failed
                    transaction.Status = TransactionStatus.Failed;
                    transaction.FailedAt = DateTime.UtcNow;
                    transaction.FailureReason = "Max retry attempts exceeded";
                    await _transactionRepository.UpdateAsync(transaction);
                }
            }

            await _queueRepository.UpdateAsync(queueItem);
            await _queueRepository.SaveChangesAsync();

            return BaseResponse<string>.Succes("Transaction processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction state for {TransactionId}", transactionId);
            return BaseResponse<string>.Failure("Failed to process transaction state");
        }
    }

    private async Task<bool> ProcessStatusTransition(Transaction transaction, Account account, TransactionStatus targetStatus)
    {
        try
        {
            switch (targetStatus)
            {
                case TransactionStatus.Pending:
                    return await ProcessToPending(transaction, account);
                case TransactionStatus.Sent:
                    return await ProcessToSent(transaction, account);
                case TransactionStatus.Completed:
                    return await ProcessToCompleted(transaction, account);
                case TransactionStatus.Failed:
                    return await ProcessToFailed(transaction, account);
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in status transition for transaction {TransactionId}", transaction.Id);
            return false;
        }
    }

    private async Task<bool> ProcessToPending(Transaction transaction, Account account)
    {
        // Validate transaction (fraud checks, balance verification, etc.)
        _logger.LogInformation("Processing transaction {TransactionRef} to Pending status", transaction.TransactionReference);

        // Simulate bank validation process
        await Task.Delay(Random.Shared.Next(1000, 3000));

        // Update transaction status
        transaction.Status = TransactionStatus.Pending;
        transaction.PendingAt = DateTime.UtcNow;
        transaction.StatusMessage = "Transaction validated and pending processing";

        await _transactionRepository.UpdateAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // Enqueue for next status (Sent)
        await EnqueueTransactionAsync(transaction.Id, TransactionStatus.Sent, 1);

        return true;
    }

    private async Task<bool> ProcessToSent(Transaction transaction, Account account)
    {
        // Simulate sending to recipient bank
        _logger.LogInformation("Sending transaction {TransactionRef} to recipient bank", transaction.TransactionReference);

        await Task.Delay(Random.Shared.Next(2000, 5000));

        // Simulate NIP/Interbank communication
        var success = SimulateInterbankCommunication();

        if (success)
        {
            transaction.Status = TransactionStatus.Sent;
            transaction.SentAt = DateTime.UtcNow;
            transaction.SessionId = Guid.NewGuid().ToString();
            transaction.StatusMessage = "Sent to recipient bank and acknowledged";

            await _transactionRepository.UpdateAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            // Enqueue for completion
            await EnqueueTransactionAsync(transaction.Id, TransactionStatus.Completed, 1);
            return true;
        }

        return false;
    }

    private async Task<bool> ProcessToCompleted(Transaction transaction, Account account)
    {
        _logger.LogInformation("Completing transaction {TransactionRef}", transaction.TransactionReference);

        // Simulate recipient bank processing
        await Task.Delay(Random.Shared.Next(1000, 4000));

        var success = SimulateRecipientBankProcessing();

        if (success)
        {
            transaction.Status = TransactionStatus.Completed;
            transaction.CompletedAt = DateTime.UtcNow;
            transaction.ResponseCode = "00";
            transaction.BankReferenceNumber = $"BNK-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
            transaction.StatusMessage = "Transaction completed successfully";

            // Update account balances
            if (transaction.Type == TransactionType.Deposit)
            {
                account.Balance += transaction.Amount;
                account.PendingBalance -= transaction.Amount;
            }
            else if (transaction.Type == TransactionType.Transfer)
            {
                account.PendingBalance -= transaction.Amount;
            }

            account.UpdatedAt = DateTime.UtcNow;

            await _transactionRepository.UpdateAsync(transaction);
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();

            return true;
        }

        return false;
    }

    private async Task<bool> ProcessToFailed(Transaction transaction, Account account)
    {
        _logger.LogInformation("Processing transaction {TransactionRef} to Failed status", transaction.TransactionReference);

        transaction.Status = TransactionStatus.Failed;
        transaction.FailedAt = DateTime.UtcNow;
        transaction.ResponseCode = "01"; // Generic failure code
        transaction.FailureReason = transaction.FailureReason ?? "Transaction processing failed";

        // Reverse balance changes
        if (transaction.Type == TransactionType.Deposit)
        {
            account.PendingBalance -= transaction.Amount;
        }
        else if (transaction.Type == TransactionType.Transfer)
        {
            account.Balance += transaction.Amount;
            account.PendingBalance -= transaction.Amount;
        }

        account.UpdatedAt = DateTime.UtcNow;

        await _transactionRepository.UpdateAsync(transaction);
        await _accountRepository.UpdateAsync(account);
        await _accountRepository.SaveChangesAsync();

        return true;
    }

    private bool SimulateInterbankCommunication()
    {
        // Simulate 95% success rate for interbank communication
        return Random.Shared.NextDouble() > 0.05;
    }

    private bool SimulateRecipientBankProcessing()
    {
        // Simulate 90% success rate for recipient bank processing
        return Random.Shared.NextDouble() > 0.10;
    }

    private DateTime CalculateProcessingTime(TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.Pending => DateTime.UtcNow.AddSeconds(10),
            TransactionStatus.Sent => DateTime.UtcNow.AddSeconds(30),
            TransactionStatus.Completed => DateTime.UtcNow.AddMinutes(2),
            _ => DateTime.UtcNow.AddMinutes(5)
        };
    }

    private DateTime CalculateRetryTime(int retryCount)
    {
        // Exponential backoff: 30s, 1m, 2m
        var seconds = Math.Pow(2, retryCount) * 30;
        return DateTime.UtcNow.AddSeconds(seconds);
    }

    private string DetermineQueueType(decimal amount)
    {
        return amount switch
        {
            > 1000000 => "Express",
            > 100000 => "Priority",
            _ => "Standard"
        };
    }

    public async Task<BaseResponse<List<TransactionQueue>>> GetQueueStatusAsync()
    {
        try
        {
            var pendingItems = await _queueRepository.GetPendingItemsAsync(50);
            return BaseResponse<List<TransactionQueue>>.Succes(pendingItems.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue status");
            return BaseResponse<List<TransactionQueue>>.Failure("Failed to get queue status");
        }
    }

    public async Task<BaseResponse<string>> RetryFailedTransactionAsync(Guid transactionId)
    {
        try
        {
            var queueItem = await _queueRepository.GetByTransactionIdAsync(transactionId);
            if (queueItem == null)
            {
                return BaseResponse<string>.Failure("Transaction not found in queue");
            }

            queueItem.RetryCount = 0;
            queueItem.ProcessedAt = null;
            queueItem.IsProcessing = false;
            queueItem.ScheduledProcessTime = DateTime.UtcNow.AddSeconds(10);
            queueItem.ErrorMessage = null;

            await _queueRepository.UpdateAsync(queueItem);
            await _queueRepository.SaveChangesAsync();

            return BaseResponse<string>.Succes("Transaction retry scheduled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying transaction {TransactionId}", transactionId);
            return BaseResponse<string>.Failure("Failed to retry transaction");
        }
    }

    public async Task<BaseResponse<int>> GetQueueCountAsync()
    {
        try
        {
            var count = await _queueRepository.GetQueueCountAsync();
            return BaseResponse<int>.Succes(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue count");
            return BaseResponse<int>.Failure("Failed to get queue count");
        }
    }
}
using FluentValidation;
using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;
using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Application.Interfaces.IServices;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InstantSyncBackend.Persistence.Services;

public class TransactionService(
    IAccountRepository _accountRepository,
    ITransactionRepository _transactionRepository,
    IValidator<TransferDto> _transferValidator,
    IValidator<AddFundsDto> _addFundsValidator,
    ILogger<TransactionService> _logger) : ITransactionService
{
    public async Task<BaseResponse<TransactionResponseDto>> TransferFundsAsync(string userId, TransferDto transferDto)
    {
        try
        {
            // Validate request
            var validationResult = await _transferValidator.ValidateAsync(transferDto);
            if (!validationResult.IsValid)
            {
                return BaseResponse<TransactionResponseDto>.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                return BaseResponse<TransactionResponseDto>.Failure("Account not found");
            }

            if (account.Balance < transferDto.Amount)
            {
                return BaseResponse<TransactionResponseDto>.Failure("Insufficient funds");
            }

            // Validate beneficiary account exists
            var beneficiaryAccount = await _accountRepository.GetByAccountNumberAsync(transferDto.BeneficiaryAccountNumber);
            if (beneficiaryAccount == null)
            {
                return BaseResponse<TransactionResponseDto>.Failure("Beneficiary account not found");
            }

            // Prevent self-transfer
            if (account.AccountNumber == transferDto.BeneficiaryAccountNumber)
            {
                return BaseResponse<TransactionResponseDto>.Failure("Cannot transfer to your own account");
            }

            // Create transaction record
            var transaction = new Transaction
            {
                TransactionReference = $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                AccountId = account.Id,
                Amount = transferDto.Amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                BeneficiaryAccountNumber = transferDto.BeneficiaryAccountNumber,
                BeneficiaryBankName = transferDto.BeneficiaryBankName,
                Description = transferDto.Description,
                CompletedAt = DateTime.UtcNow
            };

            // Update account balances immediately (commit the transfer)
            account.Balance -= transferDto.Amount;
            // don't keep amount in pending since transfer is immediate
            account.UpdatedAt = DateTime.UtcNow;

            // Credit beneficiary immediately
            beneficiaryAccount.Balance += transferDto.Amount;
            beneficiaryAccount.UpdatedAt = DateTime.UtcNow;

            // Create a corresponding transaction record for the beneficiary so both sides are tracked
            var beneficiaryTransaction = new Transaction
            {
                TransactionReference = $"{transaction.TransactionReference}-RCV",
                AccountId = beneficiaryAccount.Id,
                Amount = transaction.Amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                BeneficiaryAccountNumber = beneficiaryAccount.AccountNumber,
                BeneficiaryBankName = transaction.BeneficiaryBankName,
                Description = $"Credit from transfer {transaction.TransactionReference}",
                CompletedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.AddAsync(beneficiaryTransaction);
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.UpdateAsync(beneficiaryAccount);
            await _accountRepository.SaveChangesAsync();

            // Simulate NIP response
            var response = new TransactionResponseDto
            {
                TransactionReference = transaction.TransactionReference,
                ResponseCode = "00",
                ResponseDescription = "Transaction completed",
                SettlementDate = DateTime.UtcNow,
                Status = transaction.Status,
                Amount = transferDto.Amount,
                OriginatorAccountNumber = account.AccountNumber,
                BeneficiaryAccountNumber = transferDto.BeneficiaryAccountNumber
            };

            // Do not start async status update since transfer is immediate

            return BaseResponse<TransactionResponseDto>.Succes(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer for user {UserId}", userId);
            return BaseResponse<TransactionResponseDto>.Failure("An error occurred while processing the transfer");
        }
    }

    public async Task<BaseResponse<TransactionResponseDto>> AddFundsAsync(string userId, AddFundsDto addFundsDto)
    {
        try
        {
            // Validate request
            var validationResult = await _addFundsValidator.ValidateAsync(addFundsDto);
            if (!validationResult.IsValid)
            {
                return BaseResponse<TransactionResponseDto>.ValidationFailure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                return BaseResponse<TransactionResponseDto>.Failure("Account not found");
            }

            // Create transaction record
            var transaction = new Transaction
            {
                TransactionReference = $"DEP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                AccountId = account.Id,
                Amount = addFundsDto.Amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = $"Deposit via {addFundsDto.PaymentMethod}"
            };

            account.PendingBalance += addFundsDto.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();

            var response = new TransactionResponseDto
            {
                TransactionReference = transaction.TransactionReference,
                ResponseCode = "00",
                ResponseDescription = "Deposit processing",
                SettlementDate = DateTime.UtcNow.AddMinutes(1),
                Amount = addFundsDto.Amount,
                OriginatorAccountNumber = account.AccountNumber,
                BeneficiaryAccountNumber = account.AccountNumber
            };

            // Start async transaction status update (simulated)
            _ = UpdateTransactionStatusAsync(transaction.Id);

            return BaseResponse<TransactionResponseDto>.Succes(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit for user {UserId}", userId);
            return BaseResponse<TransactionResponseDto>.Failure("An error occurred while processing the deposit");
        }
    }

    public async Task<BaseResponse<List<TransactionHistoryDto>>> GetTransactionHistoryAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var transactions = await _transactionRepository.GetUserTransactionsAsync(userId, startDate, endDate);

            // Map to DTOs first
            var transactionDtos = transactions.Select(t => new TransactionHistoryDto
            {
                TransactionReference = t.TransactionReference,
                Date = t.CreatedAt,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                BeneficiaryAccountNumber = t.BeneficiaryAccountNumber,
                BeneficiaryBankName = t.BeneficiaryBankName,
                Description = t.Description,
                EntryType = string.Empty
            }).ToList();

            // Determine entry type (Sent / Received) from perspective of the user's account
            var userAccount = await _accountRepository.GetByUserIdAsync(userId);
            if (userAccount != null)
            {
                foreach (var dto in transactionDtos)
                {
                    var entity = transactions.FirstOrDefault(t => t.TransactionReference == dto.TransactionReference);
                    if (entity == null)
                    {
                        dto.EntryType = "Unknown";
                        continue;
                    }

                    if (entity.Type == TransactionType.Deposit)
                    {
                        dto.EntryType = "Received";
                        continue;
                    }

                    // For transfers, determine if the record represents a sent or received transaction
                    if (entity.Type == TransactionType.Transfer)
                    {
                        if (entity.AccountId == userAccount.Id && !string.IsNullOrEmpty(entity.BeneficiaryAccountNumber) && entity.BeneficiaryAccountNumber == userAccount.AccountNumber)
                        {
                            dto.EntryType = "Received";
                        }
                        else if (entity.AccountId == userAccount.Id)
                        {
                            dto.EntryType = "Sent";
                        }
                        else if (!string.IsNullOrEmpty(entity.BeneficiaryAccountNumber) && entity.BeneficiaryAccountNumber == userAccount.AccountNumber)
                        {
                            dto.EntryType = "Received";
                        }
                        else
                        {
                            dto.EntryType = "Sent";
                        }
                    }
                    else
                    {
                        dto.EntryType = "Unknown";
                    }
                }
            }

            return BaseResponse<List<TransactionHistoryDto>>.Succes(transactionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction history for user {UserId}", userId);
            return BaseResponse<List<TransactionHistoryDto>>.Failure("An error occurred while retrieving transaction history");
        }
    }

    public async Task<BaseResponse<TransactionHistoryDto>> GetTransactionStatusAsync(string userId, string transactionReference)
    {
        try
        {
            var transaction = await _transactionRepository.GetUserTransactionByReferenceAsync(userId, transactionReference);

            if (transaction == null)
            {
                return BaseResponse<TransactionHistoryDto>.Failure("Transaction not found");
            }

            var transactionDto = new TransactionHistoryDto
            {
                TransactionReference = transaction.TransactionReference,
                Date = transaction.CreatedAt,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Status = transaction.Status.ToString(),
                BeneficiaryAccountNumber = transaction.BeneficiaryAccountNumber,
                BeneficiaryBankName = transaction.BeneficiaryBankName,
                Description = transaction.Description
            };

            return BaseResponse<TransactionHistoryDto>.Succes(transactionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction status for reference {TransactionReference}", transactionReference);
            return BaseResponse<TransactionHistoryDto>.Failure("An error occurred while retrieving transaction status");
        }
    }

    private async Task UpdateTransactionStatusAsync(Guid transactionId)
    {
        try
        {
            // Simulate processing delay
            await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(5, 15)));

            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return;

            var senderAccount = await _accountRepository.GetByIdAsync(transaction.AccountId);
            if (senderAccount == null) return;

            // Get beneficiary account for transfer transactions
            Account? beneficiaryAccount = null;
            if (transaction.Type == TransactionType.Transfer)
            {
                beneficiaryAccount = await _accountRepository.GetByAccountNumberAsync(transaction.BeneficiaryAccountNumber);
                if (beneficiaryAccount == null) return;
            }

            // Simulate success rate (90% success, 10% failure)
            var success = Random.Shared.NextDouble() > 0.1;

            transaction.Status = success ? TransactionStatus.Completed : TransactionStatus.Failed;
            transaction.CompletedAt = DateTime.UtcNow;

            if (success)
            {
                // For completed transactions, move amount from pending to actual balance
                if (transaction.Type == TransactionType.Deposit)
                {
                    senderAccount.Balance += transaction.Amount;
                    senderAccount.PendingBalance -= transaction.Amount;
                }
                else if (transaction.Type == TransactionType.Transfer)
                {
                    senderAccount.PendingBalance -= transaction.Amount;
                    // Credit beneficiary account
                    beneficiaryAccount.Balance += transaction.Amount;

                    // Create a corresponding transaction record for the beneficiary so both sides are tracked
                    var beneficiaryTransaction = new Transaction
                    {
                        TransactionReference = $"{transaction.TransactionReference}-RCV",
                        AccountId = beneficiaryAccount.Id,
                        Amount = transaction.Amount,
                        Type = TransactionType.Transfer,
                        Status = TransactionStatus.Completed,
                        BeneficiaryAccountNumber = beneficiaryAccount.AccountNumber,
                        BeneficiaryBankName = transaction.BeneficiaryBankName,
                        Description = $"Credit from transfer {transaction.TransactionReference}",
                        CompletedAt = DateTime.UtcNow
                    };

                    await _transactionRepository.AddAsync(beneficiaryTransaction);
                }
            }
            else
            {
                // For failed transactions, reverse the pending amounts
                if (transaction.Type == TransactionType.Deposit)
                {
                    senderAccount.PendingBalance -= transaction.Amount;
                }
                else if (transaction.Type == TransactionType.Transfer)
                {
                    senderAccount.Balance += transaction.Amount;
                    senderAccount.PendingBalance -= transaction.Amount;
                }
            }

            senderAccount.UpdatedAt = DateTime.UtcNow;
            if (beneficiaryAccount != null)
            {
                beneficiaryAccount.UpdatedAt = DateTime.UtcNow;
            }

            await _transactionRepository.UpdateAsync(transaction);
            await _accountRepository.UpdateAsync(senderAccount);
            if (beneficiaryAccount != null)
            {
                await _accountRepository.UpdateAsync(beneficiaryAccount);
            }
            await _accountRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction status for transaction {TransactionId}", transactionId);
        }
    }
}
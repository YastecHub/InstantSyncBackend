using InstantSyncBackend.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantSyncBackend.Domain.Entities;

public class Transaction : BaseEntity
{
    [Required]
    public string TransactionReference { get; set; }
    
    [Required]
    public Guid AccountId { get; set; }
    public Account Account { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public TransactionType Type { get; set; }
    
    [Required]
    public TransactionStatus Status { get; set; }
    
    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryBankName { get; set; }
    public string? Description { get; set; }
    
    // Enhanced state tracking timestamps
    public DateTime? InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PendingAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    
    // Additional tracking fields
    public string? FailureReason { get; set; }
    public string? StatusMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    
    // NIP/Interbank specific fields
    public string? SessionId { get; set; }          // Bank session ID for tracking
    public string? ResponseCode { get; set; }       // Bank response code (00, 01, etc.)
    public string? BankReferenceNumber { get; set; } // Reference from recipient bank
}

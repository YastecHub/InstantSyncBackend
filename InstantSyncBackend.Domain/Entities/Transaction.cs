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
    public DateTime? CompletedAt { get; set; }
}

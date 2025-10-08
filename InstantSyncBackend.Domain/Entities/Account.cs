using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantSyncBackend.Domain.Entities;

public class Account : BaseEntity
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public ApplicationUser User { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendingBalance { get; set; }
    
    [Required]
    public string AccountNumber { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
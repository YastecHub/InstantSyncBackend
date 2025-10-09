using InstantSyncBackend.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantSyncBackend.Domain.Entities;

public class TransactionQueue : BaseEntity
{
    [Required]
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    [Required]
    public TransactionStatus CurrentStatus { get; set; }
    
    [Required]
    public TransactionStatus NextStatus { get; set; }
    
    [Required]
    public DateTime ScheduledProcessTime { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public int Priority { get; set; } = 1; // 1 = Normal, 2 = High, 3 = Urgent
    
    public int RetryCount { get; set; } = 0;
    
    public int MaxRetries { get; set; } = 3;
    
    public bool IsProcessing { get; set; } = false;
    
    public string? ProcessingNode { get; set; } // For load balancing
    
    public string? ErrorMessage { get; set; }
    
    // Queue metadata
    public string QueueType { get; set; } = "Standard"; // Standard, Express, Bulk
}
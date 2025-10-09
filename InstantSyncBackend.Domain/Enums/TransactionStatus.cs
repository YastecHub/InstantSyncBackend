namespace InstantSyncBackend.Domain.Enums;

public enum TransactionStatus
{
    Initiated = 1,      // Customer initiated the transaction
    Pending = 2,        // Sitting with the sender bank (validation, fraud checks)
    Sent = 3,          // Sent and acknowledged by the recipient bank
    Completed = 4,      // Successfully processed by the recipient bank
    Failed = 5         // Transaction failed at any stage
}

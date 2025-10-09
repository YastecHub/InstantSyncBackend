namespace InstantSyncBackend.Application.Dtos;

public class AccountDetailsDto
{
    public string UserId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableToSpend => Balance;
}
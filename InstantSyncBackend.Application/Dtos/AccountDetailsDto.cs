namespace InstantSyncBackend.Application.Dtos;

public class ApplicationUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}

public class AccountDetailsDto
{
    public string UserId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableToSpend => Balance;

    // Include user details
    public ApplicationUserDto? User { get; set; }
}
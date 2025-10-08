namespace InstantSyncBackend.Application.Dtos;

public class TransferDto
{
    public string BeneficiaryAccountNumber { get; set; }
    public string BeneficiaryBankName { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class AddFundsDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
}

public class TransactionResponseDto
{
    public string TransactionReference { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseDescription { get; set; }
    public DateTime SettlementDate { get; set; }
    public decimal Amount { get; set; }
    public string OriginatorAccountNumber { get; set; }
    public string BeneficiaryAccountNumber { get; set; }
}

public class TransactionHistoryDto
{
    public string TransactionReference { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryBankName { get; set; }
    public string? Description { get; set; }
}
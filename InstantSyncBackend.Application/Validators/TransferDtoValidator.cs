using FluentValidation;
using InstantSyncBackend.Application.Dtos;

namespace InstantSyncBackend.Application.Validators;

public class TransferDtoValidator : AbstractValidator<TransferDto>
{
    public TransferDtoValidator()
    {
        RuleFor(x => x.BeneficiaryAccountNumber)
            .NotEmpty().WithMessage("Beneficiary account number is required")
            .Matches("^\\d{8,20}$").WithMessage("Beneficiary account number must be numeric and between 8 and 20 digits");

        RuleFor(x => x.BeneficiaryBankName)
            .NotEmpty().WithMessage("Beneficiary bank name is required")
            .MaximumLength(100).WithMessage("Bank name must be 100 characters or fewer");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description must be 250 characters or fewer");
    }
}

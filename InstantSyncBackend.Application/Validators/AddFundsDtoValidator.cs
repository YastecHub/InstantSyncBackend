using FluentValidation;
using InstantSyncBackend.Application.Dtos;

namespace InstantSyncBackend.Application.Validators;

public class AddFundsDtoValidator : AbstractValidator<AddFundsDto>
{
    public AddFundsDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .MaximumLength(100).WithMessage("Payment method must be 100 characters or fewer");
    }
}

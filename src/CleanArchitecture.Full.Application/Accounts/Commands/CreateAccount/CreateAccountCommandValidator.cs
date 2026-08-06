using FluentValidation;

namespace CleanArchitecture.Full.Application.Accounts.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HolderName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "Active" or "Inactive")
            .WithMessage("Status debe ser 'Active' or 'Inactive'.");
    }
}

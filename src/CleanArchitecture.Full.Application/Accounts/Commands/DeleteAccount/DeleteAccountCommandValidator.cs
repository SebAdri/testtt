using FluentValidation;

namespace CleanArchitecture.Full.Application.Accounts.Commands.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

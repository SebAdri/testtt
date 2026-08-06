using CleanArchitecture.Full.Domain;
using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Commands.DeleteAccount;

public class DeleteAccountCommandHandler(IAccountRepository repository) : IRequestHandler<DeleteAccountCommand, bool>
{
    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            return false;
        }

        repository.Remove(account);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}

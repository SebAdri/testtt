using CleanArchitecture.Full.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Full.Application.Accounts.Commands.UpdateAccount;

public class UpdateAccountCommandHandler(IAccountRepository repository, ILogger<UpdateAccountCommandHandler> logger)
    : IRequestHandler<UpdateAccountCommand, AccountDto?>
{
    private const decimal MinimumRecommendedBalance = 100m;

    public async Task<AccountDto?> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
        {
            return null;
        }

        account.AccountNumber = request.AccountNumber;
        account.HolderName = request.HolderName;
        account.Balance = request.Balance;
        account.Status = request.Status;

        repository.Update(account);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "El Account {AccountId} ({AccountNumber}) actualizado",
            account.Id,
            account.AccountNumber);

        if (account.Balance < MinimumRecommendedBalance)
        {
            // Regla de negocio "blanda": no bloquea la actualización, solo advierte.
            logger.LogWarning(
                "Account {AccountNumber} updated with balance {Balance} below the recommended minimum of {MinimumBalance}",
                account.AccountNumber,
                account.Balance,
                MinimumRecommendedBalance);
        }

        return account.ToDto();
    }
}

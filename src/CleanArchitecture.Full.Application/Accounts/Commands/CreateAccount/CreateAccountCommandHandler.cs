using CleanArchitecture.Full.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Full.Application.Accounts.Commands.CreateAccount;

public class CreateAccountCommandHandler(IAccountRepository repository, ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private const decimal MinimumRecommendedBalance = 100m;

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = request.AccountNumber,
            HolderName = request.HolderName,
            Balance = request.Balance,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(account, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "El Account {AccountId} ({AccountNumber}) creada para {HolderName}",
            account.Id,
            account.AccountNumber,
            account.HolderName);

        if (account.Balance < MinimumRecommendedBalance)
        {
            // Regla de negocio "blanda": no bloquea la creación, solo advierte.
            logger.LogWarning(
                "El Account {AccountNumber} creado con balance {Balance} por debajo del recomendado {MinimumBalance}",
                account.AccountNumber,
                account.Balance,
                MinimumRecommendedBalance);
        }

        return account.ToDto();
    }
}

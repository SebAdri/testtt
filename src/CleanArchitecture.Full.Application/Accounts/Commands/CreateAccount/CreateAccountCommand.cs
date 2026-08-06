using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    string AccountNumber,
    string HolderName,
    decimal Balance,
    string Status) : IRequest<AccountDto>;

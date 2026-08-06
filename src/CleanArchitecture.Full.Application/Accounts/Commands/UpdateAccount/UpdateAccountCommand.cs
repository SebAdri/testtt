using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(
    Guid Id,
    string AccountNumber,
    string HolderName,
    decimal Balance,
    string Status) : IRequest<AccountDto?>;

using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid Id) : IRequest<bool>;

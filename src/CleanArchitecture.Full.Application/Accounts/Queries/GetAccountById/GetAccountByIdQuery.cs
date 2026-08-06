using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto?>;

using MediatR;

namespace CleanArchitecture.Full.Application.Accounts.Queries.GetAllAccounts;

public record GetAllAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;

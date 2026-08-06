using CleanArchitecture.Full.Domain;

namespace CleanArchitecture.Full.Application.Accounts;

public static class AccountMappingExtensions
{
    public static AccountDto ToDto(this Account account) =>
        new(account.Id, account.AccountNumber, account.HolderName, account.Balance, account.Status, account.CreatedAt);
}

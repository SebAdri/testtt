namespace CleanArchitecture.Full.Application.Accounts;

public record AccountDto(
    Guid Id,
    string AccountNumber,
    string HolderName,
    decimal Balance,
    string Status,
    DateTime CreatedAt);

using CleanArchitecture.Full.Domain;

namespace CleanArchitecture.Full.Infrastructure.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Accounts.Any())
        {
            return;
        }

        context.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0001",
                HolderName = "Ada Lovelace",
                Balance = 15000.50m,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0002",
                HolderName = "Alan Turing",
                Balance = 8320.00m,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0003",
                HolderName = "Grace Hopper",
                Balance = 0.00m,
                Status = "Inactive",
                CreatedAt = DateTime.UtcNow
            });

        context.SaveChanges();
    }
}

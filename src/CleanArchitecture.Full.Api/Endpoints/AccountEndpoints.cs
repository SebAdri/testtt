using CleanArchitecture.Full.Application.Accounts;
using CleanArchitecture.Full.Application.Accounts.Commands.CreateAccount;
using CleanArchitecture.Full.Application.Accounts.Commands.DeleteAccount;
using CleanArchitecture.Full.Application.Accounts.Commands.UpdateAccount;
using CleanArchitecture.Full.Application.Accounts.Queries.GetAllAccounts;
using CleanArchitecture.Full.Application.Accounts.Queries.GetAccountById;
using MediatR;

namespace CleanArchitecture.Full.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/minimal/accounts").WithTags("Accounts (Minimal API)");

        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetAllAccountsQuery(), cancellationToken)))
            .Produces<IReadOnlyList<AccountDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var account = await sender.Send(new GetAccountByIdQuery(id), cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        })
            .Produces<AccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("", async (CreateAccountCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var account = await sender.Send(command, cancellationToken);
            return Results.Created($"api/minimal/accounts/{account.Id}", account);
        })
            .Produces<AccountDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPut("{id:guid}", async (Guid id, UpdateAccountBody body, ISender sender, CancellationToken cancellationToken) =>
        {
            var account = await sender.Send(new UpdateAccountCommand(id, body.AccountNumber, body.HolderName, body.Balance, body.Status), cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        })
            .Produces<AccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapDelete("{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var deleted = await sender.Send(new DeleteAccountCommand(id), cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}

public record UpdateAccountBody(string AccountNumber, string HolderName, decimal Balance, string Status);

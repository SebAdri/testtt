using FluentValidation;

namespace CleanArchitecture.Full.Application.Accounts.Queries.GetAccountById;

public class GetAccountByIdQueryValidator : AbstractValidator<GetAccountByIdQuery>
{
    public GetAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Account;

public class GetUserDetailQueryValidator : AbstractValidator<GetUserDetailQuery>
{
    public GetUserDetailQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
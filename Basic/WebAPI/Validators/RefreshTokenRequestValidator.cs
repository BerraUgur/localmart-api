using FluentValidation;
using WebAPI.ModelViews;

namespace WebAPI.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token cannot be empty.")
            .NotNull().WithMessage("Token cannot be null.");
    }
}

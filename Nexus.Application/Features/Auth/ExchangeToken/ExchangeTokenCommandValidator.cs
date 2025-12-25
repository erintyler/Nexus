using FluentValidation;

namespace Nexus.Application.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandValidator : AbstractValidator<ExchangeTokenCommand>
{
    public ExchangeTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required.");
    }
}


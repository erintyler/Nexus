using FluentValidation;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.MigrateTag;

public class MigrateTagCommandValidator : AbstractValidator<MigrateTagCommand>
{
    public const int MaxSearchTermLength = 60;

    public MigrateTagCommandValidator()
    {
        RuleFor(x => x.Source)
            .NotNull()
            .WithMessage("Source tag must be provided.");

        RuleFor(x => x.Target)
            .NotNull()
            .WithMessage("Target tag must be provided.");

        RuleFor(x => x)
            .Must(cmd => cmd.Source.Type != cmd.Target.Type || cmd.Source.Value != cmd.Target.Value)
            .WithMessage("Source and Target tags must be different.");
    }
}
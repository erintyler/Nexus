using FluentValidation;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTags;

public class GetTagsQueryValidator : BasePaginationRequestValidator<GetTagsQuery>
{
    public const int MaxSearchTermLength = 60;

    public GetTagsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(MaxSearchTermLength)
            .WithMessage($"Search term cannot exceed {MaxSearchTermLength} characters.");
    }
}
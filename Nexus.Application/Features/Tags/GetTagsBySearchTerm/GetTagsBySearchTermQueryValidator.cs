using FluentValidation;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTagsBySearchTerm;

public class GetTagsBySearchTermQueryValidator : PaginationRequestValidator<GetTagsBySearchTermQuery>
{
    public const int MinSearchTermLength = 2;
    
    public GetTagsBySearchTermQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Search term is required")
            .MinimumLength(MinSearchTermLength)
            .WithMessage($"Search term must be at least {MinSearchTermLength} characters long.");
    }
}
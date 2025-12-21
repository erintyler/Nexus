using FluentValidation;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public class GetImageHistoryQueryValidator : PaginationRequestValidator<GetImageHistoryQuery>
{
    public GetImageHistoryQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID must be provided.");

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateFrom must be less than or equal to DateTo.");
        
        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .When(x => x.DateFrom.HasValue)
            .WithMessage("DateFrom cannot be in the future.");
        
        RuleFor(x => x.DateTo)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .When(x => x.DateTo.HasValue)
            .WithMessage("DateTo cannot be in the future.");
    }
}
using FluentValidation;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public class GetImageHistoryQueryValidator : BasePaginationRequestValidator<GetHistoryQuery>
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
    }
}
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTopTags;

public class GetTopTagsQueryValidator : PaginationRequestValidator<GetTopTagsQuery>
{
    public GetTopTagsQueryValidator()
    {
    }
}
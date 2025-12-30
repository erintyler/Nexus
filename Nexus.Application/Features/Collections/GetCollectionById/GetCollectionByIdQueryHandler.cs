using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Wolverine.Marten;

namespace Nexus.Application.Features.Collections.GetCollectionById;

public static class GetCollectionByIdQueryHandler
{
    public static Result<CollectionReadModel> Handle(
        GetCollectionByIdQuery query,
        [ReadAggregate(Required = false)] CollectionReadModel? collection)
    {
        return collection is null ? Result.Failure<CollectionReadModel>(CollectionErrors.NotFound) : Result.Success(collection);
    }
}

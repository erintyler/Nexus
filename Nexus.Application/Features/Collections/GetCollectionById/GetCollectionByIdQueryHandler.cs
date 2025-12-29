using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Wolverine.Marten;

namespace Nexus.Application.Features.Collections.GetCollectionById;

public static class GetCollectionByIdQueryHandler
{
    public static Task<Result<CollectionReadModel>> HandleAsync(
        GetCollectionByIdQuery query,
        [ReadAggregate] CollectionReadModel? collection)
    {
        if (collection == null)
        {
            return Task.FromResult(Result.Failure<CollectionReadModel>(CollectionErrors.NotFound));
        }

        return Task.FromResult(Result.Success(collection));
    }
}

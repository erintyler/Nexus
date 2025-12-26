using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Wolverine.Marten;

namespace Nexus.Application.Features.Collections.GetCollectionById;

public class GetCollectionByIdQueryHandler
{
    public static Result<CollectionReadModel> HandleAsync(
        GetCollectionByIdQuery query,
        [ReadAggregate] CollectionReadModel? collection)
    {
        if (collection == null)
        {
            return Result.Failure<CollectionReadModel>(CollectionErrors.NotFound);
        }

        return Result.Success(collection);
    }
}

using Marten;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Common;

namespace Nexus.Application.Features.Collections.GetCollectionById;

public class GetCollectionByIdQueryHandler
{
    public static async Task<Result<CollectionReadModel>> HandleAsync(
        GetCollectionByIdQuery query,
        IDocumentSession session,
        CancellationToken ct)
    {
        var collection = await session.LoadAsync<CollectionReadModel>(query.Id, ct);

        if (collection == null)
        {
            return Result.Failure<CollectionReadModel>(Domain.Errors.CollectionErrors.NotFound);
        }

        return Result.Success(collection);
    }
}

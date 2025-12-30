using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Wolverine.Marten;

namespace Nexus.Application.Features.Collections.RemoveImagePostFromCollection;

public class RemoveImagePostFromCollectionCommandHandler
{
    public static (Result, Events) HandleAsync(
        RemoveImagePostFromCollectionCommand request,
        [WriteAggregate(Required = false)] Collection? collection)
    {
        if (collection is null)
        {
            return (CollectionErrors.NotFound, []);
        }

        // Let the aggregate handle validation and event creation
        var removeResult = collection.RemoveImagePost(request.ImagePostId);

        if (removeResult.IsFailure)
        {
            return (Result.Failure(removeResult.Errors), []);
        }

        return (Result.Success(), [removeResult.Value]);
    }
}

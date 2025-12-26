using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.Collections.RemoveImagePostFromCollection;

public class RemoveImagePostFromCollectionCommandHandler
{
    public static (Result, Events) HandleAsync(
        RemoveImagePostFromCollectionCommand request,
        [WriteAggregate(OnMissing = OnMissing.ProblemDetailsWith404)] Collection collection)
    {
        // Let the aggregate handle validation and event creation
        var removeResult = collection.RemoveImagePost(request.ImagePostId);

        if (removeResult.IsFailure)
        {
            return (Result.Failure(removeResult.Errors), []);
        }

        return (Result.Success(), [removeResult.Value]);
    }
}

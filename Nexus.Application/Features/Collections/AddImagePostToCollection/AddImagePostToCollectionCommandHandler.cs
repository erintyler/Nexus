using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.Collections.AddImagePostToCollection;

public class AddImagePostToCollectionCommandHandler
{
    public static (Result, Events) HandleAsync(
        AddImagePostToCollectionCommand request,
        [WriteAggregate(OnMissing = OnMissing.ProblemDetailsWith404)] Collection collection)
    {
        // Let the aggregate handle validation and event creation
        var addResult = collection.AddImagePost(request.ImagePostId);

        if (addResult.IsFailure)
        {
            return (Result.Failure(addResult.Errors), []);
        }

        return (Result.Success(), [addResult.Value]);
    }
}

using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.Collections.AddImagePostToCollection;

public class AddImagePostToCollectionCommandHandler
{
    public static (Result, Events) Handle(
        AddImagePostToCollectionCommand request,
        [WriteAggregate(Required = false)] Collection? collection,
        [ReadAggregate(Required = false)] ImagePost? imagePost)
    {
        if (collection is null)
        {
            return (CollectionErrors.NotFound, []);
        }
        
        if (imagePost is null)
        {
            return (CollectionErrors.ImagePostDoesNotExist, []);
        }
        
        // Let the aggregate handle validation and event creation
        var addResult = collection.AddImagePost(request.ImagePostId);

        if (addResult.IsFailure)
        {
            return (Result.Failure(addResult.Errors), []);
        }

        return (Result.Success(), [addResult.Value]);
    }
}

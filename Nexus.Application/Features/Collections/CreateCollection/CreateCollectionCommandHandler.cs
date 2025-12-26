using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Wolverine.Marten;

namespace Nexus.Application.Features.Collections.CreateCollection;

public class CreateCollectionCommandHandler
{
    public static (Result<CreateCollectionResponse>, IStartStream?) HandleAsync(
        CreateCollectionCommand request,
        IUserContextService userContextService)
    {
        var userId = userContextService.GetUserId();

        // Let the aggregate handle validation and event creation
        var createEventResult = Collection.Create(userId, request.Title);

        if (createEventResult.IsFailure)
        {
            return (Result.Failure<CreateCollectionResponse>(createEventResult.Errors), null);
        }

        // Start the event stream with the created event
        var stream = MartenOps.StartStream<Collection>(createEventResult.Value);
        var response = new CreateCollectionResponse(stream.StreamId, request.Title, DateTime.UtcNow);

        return (response, stream);
    }
}

namespace Nexus.Application.Features.Collections.AddImagePostToCollection;

public record AddImagePostToCollectionCommand(Guid Id, Guid ImagePostId)
{
    // Id is the CollectionId - Wolverine uses this for the aggregate stream lookup
}


namespace Nexus.Application.Features.Collections.RemoveImagePostFromCollection;

public record RemoveImagePostFromCollectionCommand(Guid Id, Guid ImagePostId)
{
    // Id is the CollectionId - Wolverine uses this for the aggregate stream lookup
}


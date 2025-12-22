using Marten;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Common;

namespace Nexus.Application.Features.Tags.MigrateTag;

public class MigrateTagCommandHandler(IDocumentSession session)
{
    public async Task<Result> Handle(
        MigrateTagCommand request,
        CancellationToken cancellationToken)
    {
        var affectedPosts = session
            .Query<ImagePostReadModel>()
            .Where(p => p.Tags.Any(t => t.Type == request.Source.Type && t.Value == request.Source.Value))
            .ToAsyncEnumerable(cancellationToken);

        var batches = affectedPosts.Chunk(100);
    }

    public async Task MigrateBatch(ImagePostReadModel[] posts, TagDto targetTag, CancellationToken cancellationToken)
    {
        foreach (var post in posts)
        {
            
        }

        await session.SaveChangesAsync(cancellationToken);
    }
}
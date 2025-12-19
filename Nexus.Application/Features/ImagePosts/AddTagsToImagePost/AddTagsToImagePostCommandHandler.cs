using Marten;
using Nexus.Domain.Common;

namespace Nexus.Application.Features.ImagePosts.AddTagsToImagePost;

public class AddTagsToImagePostCommandHandler
{
    public static async Task<Result> Handle(AddTagsToImagePostCommand request, IDocumentSession session, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
using Marten;
using Nexus.Application.Common.Models;
using Nexus.Application.Extensions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Tags;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public static class CreateImagePostCommandHandler
{
    public static async Task<Result<ImagePost>> Handle(CreateImagePostCommand request, IDocumentSession session, CancellationToken cancellationToken = default)
    {
        var postId = Guid.NewGuid();
        var imagePostResult = ImagePost.Create(postId, request.Title);
        
        if (imagePostResult.IsFailure)
        {
            return imagePostResult;
        }
        
        var imagePost = imagePostResult.Value;
        
        foreach (var tagDto in request.Tags)
        {
            var addTagResult = AddTag(tagDto, imagePost);
            
            if (addTagResult.IsFailure)
            {
                return addTagResult.Error;
            }
        }
        
        session.Events.StartAggregateStream(imagePost);
        await session.SaveChangesAsync(cancellationToken);
        return imagePost;
    }

    private static Result AddTag(TagDto tagDto, ImagePost imagePost)
    {
        var tagResult = tagDto.ToDomainTag();
            
        return tagResult.IsFailure ? tagResult.Error : imagePost.AddTag(tagResult.Value);
    }
}
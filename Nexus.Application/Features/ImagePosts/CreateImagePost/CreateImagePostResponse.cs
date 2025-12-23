namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public record CreateImagePostResponse(Guid Id, string Title, string UploadUrl, DateTime CreatedAt);


namespace Nexus.Application.Common.Services;

public interface IImageService
{
    Task<Stream?> GetOriginalImageStreamAsync(Guid imageId, CancellationToken cancellationToken = default);
    string GetProcessedImageUrl(Guid imageId);
    Task SaveProcessedImageAsync(Guid imageId, byte[] data, CancellationToken cancellationToken = default);
    string GetThumbnailUrl(Guid imageId);
    Task SaveThumbnailAsync(Guid imageId, byte[] data, CancellationToken cancellationToken = default);
    string GenerateImageUploadUrl(Guid imageId, int expirationMinutes = 15);
}
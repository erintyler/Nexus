using Microsoft.Extensions.Options;
using Nexus.Application.Configuration.Models;

namespace Nexus.Application.Common.Services;

public class ImageService(IStorageService storageService, IOptions<ImageOptions> options) : IImageService
{
    private readonly ImageOptions _options = options.Value;

    public Task<Stream?> GetOriginalImageStreamAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        return storageService.GetObjectStreamAsync(_options.OriginalImageBucketName, imageId.ToString(), cancellationToken);
    }

    public string GetProcessedImageUrl(Guid imageId)
    {
        return $"{_options.ProcessedImagePublicDomain}/{imageId}.webp";
    }

    public Task SaveProcessedImageAsync(Guid imageId, byte[] data, CancellationToken cancellationToken = default)
    {
        var key = $"{imageId}.webp";
        return storageService.SaveObjectAsync(_options.ProcessedImageBucketName, key, data, cancellationToken);
    }

    public string GetThumbnailUrl(Guid imageId)
    {
        return $"{_options.ThumbnailPublicDomain}/{imageId}.webp";
    }

    public Task SaveThumbnailAsync(Guid imageId, byte[] data, CancellationToken cancellationToken = default)
    {
        var key = $"{imageId}.webp";
        return storageService.SaveObjectAsync(_options.ThumbnailBucketName, key, data, cancellationToken);
    }

    public string GenerateImageUploadUrl(Guid imageId, string contentType, int expirationMinutes = 15)
    {
        return storageService.GeneratePresignedUploadUrl(_options.OriginalImageBucketName, imageId.ToString(), contentType, expirationMinutes);
    }
}
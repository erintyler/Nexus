namespace Nexus.Application.Configuration.Models;

public class ImageOptions
{
    public required string OriginalImageBucketName { get; set; }
    public required string ProcessedImageBucketName { get; set; }
    public required string ProcessedImagePublicDomain { get; set; }
    public required string ThumbnailBucketName { get; set; }
    public required string ThumbnailPublicDomain { get; set; }
}
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Domain.Common;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public class ProcessImageCommandHandler
{
    public async Task<Result<ProcessImageResponse>> HandleAsync(
        ProcessImageCommand request, 
        IImageService imageService,
        IImageConversionService imageConversionService, 
        IThumbnailService thumbnailService,
        CancellationToken cancellationToken)
    {
        await using var originalImage = await imageService.GetOriginalImageStreamAsync(request.ImageId, cancellationToken);
        
        if (originalImage is null)
        {
            return ImageProcessingErrors.NotFound;
        }
        
        using var imageStream = new MemoryStream();
        await originalImage.CopyToAsync(imageStream, cancellationToken);
        imageStream.Position = 0;
        
        var webpResult = imageConversionService.ConvertToWebP(imageStream);
        
        if (webpResult.IsFailure)
        {
            return Result.Failure<ProcessImageResponse>(webpResult.Errors);
        }
        
        // Reset stream position before creating thumbnail
        using var thumbnailStream = new MemoryStream(webpResult.Value);
        var thumbnailResult = thumbnailService.CreateThumbnail(thumbnailStream);
        
        if (thumbnailResult.IsFailure)
        {
            return Result.Failure<ProcessImageResponse>(thumbnailResult.Errors);
        }
        
        var tasks = new List<Task>
        {
            imageService.SaveProcessedImageAsync(request.ImageId, webpResult.Value, cancellationToken),
            imageService.SaveThumbnailAsync(request.ImageId, thumbnailResult.Value, cancellationToken)
        };
        
        await Task.WhenAll(tasks);
        var imageUrl = imageService.GetProcessedImageUrl(request.ImageId);
        var thumbnailUrl = imageService.GetThumbnailUrl(request.ImageId);
        var response = new ProcessImageResponse(imageUrl, thumbnailUrl);
        
        return response;
    }
}
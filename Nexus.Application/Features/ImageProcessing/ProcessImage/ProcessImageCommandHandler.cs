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
        var originalImage = await imageService.GetOriginalImageStreamAsync(request.ImageId, cancellationToken);
        
        if (originalImage is null)
        {
            return ImageProcessingErrors.NotFound;
        }
        
        var webpResult = imageConversionService.ConvertToWebP(originalImage);
        
        if (webpResult.IsFailure)
        {
            return Result.Failure<ProcessImageResponse>(webpResult.Errors);
        }
        
        // Reset stream position before creating thumbnail
        originalImage.Position = 0;
        var thumbnailResult = thumbnailService.CreateThumbnail(originalImage);
        
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
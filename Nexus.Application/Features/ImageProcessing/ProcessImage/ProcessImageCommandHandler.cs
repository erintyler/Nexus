using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public class ProcessImageCommandHandler
{
    public static async Task<Events> HandleAsync(
        ProcessImageCommand request,
        IImageService imageService,
        IImageConversionService imageConversionService,
        IThumbnailService thumbnailService,
        ILogger<ProcessImageCommandHandler> logger,
        [WriteAggregate] ImagePost imagePost,
        CancellationToken cancellationToken)
    {
        await using var originalImage = await imageService.GetOriginalImageStreamAsync(request.Id, cancellationToken);

        if (originalImage is null)
        {
            logger.LogError("Original image not found for ImageId: {ImageId}", request.Id);
            return [imagePost.MarkAsFailed()];
        }

        // Read the original image data into a byte array
        var imageData = new byte[originalImage.Length];
        await originalImage.ReadExactlyAsync(imageData, cancellationToken);

        var tasks = new List<Task<Result>>
        {
            CreateAndStoreWebPAsync(imageService, imageConversionService, request.Id, imageData, cancellationToken),
            CreateAndStoreThumbnailAsync(imageService, thumbnailService, request.Id, imageData, cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        var failures = results.Where(r => r.IsFailure).ToList();

        if (failures.Count != 0)
        {
            var allErrors = failures.SelectMany(f => f.Errors).ToList();

            logger.LogError("Image processing failed for ImageId: {ImageId} with errors: {@Errors}", request.Id, allErrors);
            var markAsFailedResult = imagePost.MarkAsFailed();

            if (markAsFailedResult.IsFailure)
            {
                logger.LogError("Failed to mark ImagePost as failed for ImageId: {ImageId} with errors: {@Errors}", request.Id, markAsFailedResult.Errors);
                return [];
            }

            return [markAsFailedResult.Value];
        }

        logger.LogInformation("Image processing completed successfully for ImageId: {ImageId}", request.Id);
        var markAsCompletedResult = imagePost.MarkAsCompleted();

        if (markAsCompletedResult.IsFailure)
        {
            logger.LogError("Failed to mark ImagePost as completed for ImageId: {ImageId} with errors: {@Errors}", request.Id, markAsCompletedResult.Errors);
            return [];
        }

        return [markAsCompletedResult.Value];
    }

    private static async Task<Result> CreateAndStoreWebPAsync(
        IImageService imageService,
        IImageConversionService imageConversionService,
        Guid imageId,
        byte[] imageData,
        CancellationToken cancellationToken)
    {
        using var imageStream = new MemoryStream(imageData);
        var webpResult = await Task.Run(() => imageConversionService.ConvertToWebP(imageStream), cancellationToken);

        if (webpResult.IsFailure)
        {
            return Result.Failure(webpResult.Errors);
        }

        await imageService.SaveProcessedImageAsync(imageId, webpResult.Value, cancellationToken);
        return Result.Success();
    }

    private static async Task<Result> CreateAndStoreThumbnailAsync(
        IImageService imageService,
        IThumbnailService thumbnailService,
        Guid imageId,
        byte[] imageData,
        CancellationToken cancellationToken)
    {
        using var imageStream = new MemoryStream(imageData);
        var thumbnailResult = await Task.Run(() => thumbnailService.CreateThumbnail(imageStream), cancellationToken);

        if (thumbnailResult.IsFailure)
        {
            return Result.Failure(thumbnailResult.Errors);
        }

        await imageService.SaveThumbnailAsync(imageId, thumbnailResult.Value, cancellationToken);
        return Result.Success();
    }
}
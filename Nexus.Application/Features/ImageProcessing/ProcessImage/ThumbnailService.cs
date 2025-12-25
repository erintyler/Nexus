using System.Diagnostics;
using Nexus.Application.Constants;
using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Domain.Common;
using SkiaSharp;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public class ThumbnailService : IThumbnailService
{
    public const int ThumbnailWidth = 500;
    public const int ThumbnailHeight = 500;

    public Result<byte[]> CreateThumbnail(Stream inputImageStream)
    {
        using var activity = TelemetryConstants.ActivitySource.StartActivity("Create Thumbnail");
        using var bitmap = SKBitmap.Decode(inputImageStream);

        if (bitmap is null)
        {
            return ImageProcessingErrors.DecodeFailed;
        }

        var (thumbWidth, thumbHeight) = CalculateThumbnailDimensions(bitmap.Width, bitmap.Height);

        using var resizedBitmap = bitmap.Resize(new SKImageInfo(thumbWidth, thumbHeight), new SKSamplingOptions(SKFilterMode.Linear));

        if (resizedBitmap is null)
        {
            return ImageProcessingErrors.EncodeFailed;
        }

        using var image = SKImage.FromBitmap(resizedBitmap);
        using var webPData = image.Encode(SKEncodedImageFormat.Webp, 75);

        if (webPData is null)
        {
            return ImageProcessingErrors.EncodeFailed;
        }

        return webPData.ToArray();
    }

    private static (int width, int height) CalculateThumbnailDimensions(int originalWidth, int originalHeight)
    {
        var aspectRatio = (double)originalWidth / originalHeight;

        return originalWidth > originalHeight
            ? (ThumbnailWidth, (int)(ThumbnailWidth / aspectRatio))
            : ((int)(ThumbnailHeight * aspectRatio), ThumbnailHeight);
    }
}
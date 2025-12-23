using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Domain.Common;
using SkiaSharp;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public class ImageConversionService : IImageConversionService
{
    public Result<byte[]> ConvertToWebP(Stream inputImageStream)
    {
        using var bitmap = SKBitmap.Decode(inputImageStream);

        if (bitmap is null)
        {
            return ImageProcessingErrors.DecodeFailed;
        }
        
        using var image = SKImage.FromBitmap(bitmap);
        using var webpData = image.Encode(SKEncodedImageFormat.Webp, 85);
        
        if (webpData is null)
        {
            return ImageProcessingErrors.EncodeFailed;
        }
        
        return webpData.ToArray();
    }
}
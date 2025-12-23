using Nexus.Domain.Common;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public interface IImageConversionService
{
    Result<byte[]> ConvertToWebP(Stream inputImageStream);
}
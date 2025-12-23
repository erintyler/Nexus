using Nexus.Domain.Common;

namespace Nexus.Application.Features.ImageProcessing.ProcessImage;

public interface IThumbnailService
{
    Result<byte[]> CreateThumbnail(Stream inputImageStream);
}
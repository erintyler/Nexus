using Nexus.Domain.Common;

namespace Nexus.Application.Features.ImageProcessing.Errors;

public static class ImageProcessingErrors
{
    public static readonly Error DecodeFailed = new(
        "ImageProcessing.DecodeFailed",
        ErrorType.Failure,
        "Failed to decode the image.");

    public static readonly Error EncodeFailed = new(
        "ImageProcessing.DecodeFailed",
        ErrorType.Failure,
        "Failed to decode the image.");

    public static readonly Error NotFound = new(
        "ImageProcessing.NotFound",
        ErrorType.NotFound,
        "The specified image was not found.");
}
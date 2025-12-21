using Nexus.Domain.Common;

namespace Nexus.Domain.Errors;

public class ImagePostErrors
{
    public static readonly Error NotFound = new(
        "ImagePost.NotFound",
        ErrorType.NotFound,
        "The specified image post was not found.");
    
    public static readonly Error TitleEmpty = new(
        "ImagePost.Title.Empty",
        ErrorType.BusinessRule,
        "The title of the image post cannot be empty.");
}
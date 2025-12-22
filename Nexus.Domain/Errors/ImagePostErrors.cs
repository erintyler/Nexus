using Nexus.Domain.Common;
using Nexus.Domain.Entities;

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
    
    public static readonly Error TitleTooShort = new(
        "ImagePost.Title.TooShort",
        ErrorType.BusinessRule,
        $"Title must be at least {ImagePost.MinTitleLength} characters long.");
    
    public static readonly Error TitleTooLong = new(
        "ImagePost.Title.TooLong",
        ErrorType.BusinessRule,
        $"Title cannot exceed {ImagePost.MaxTitleLength} characters.");
    
    public static readonly Error UserIdEmpty = new(
        "ImagePost.UserId.Empty",
        ErrorType.BusinessRule,
        "User ID cannot be empty.");
    
    public static readonly Error AtLeastOneTagRequired = new(
        "ImagePost.Tags.AtLeastOneRequired",
        ErrorType.BusinessRule,
        "At least one tag is required for the image post.");
}



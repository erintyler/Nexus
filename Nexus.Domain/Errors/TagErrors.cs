using Nexus.Domain.Common;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Errors;

public static class TagErrors
{
    public static readonly Error NotFound = new(
        "Tag.NotFound",
        ErrorType.NotFound,
        "The specified tag was not found.");
    
    public static readonly Error TooShort = new(
        "Tag.TooShort",
        ErrorType.BusinessRule,
        $"The tag name must be at least {Tag.MinLength} characters long.");
    
    public static readonly Error TooLong = new(
        "Tag.TooLong",
        ErrorType.BusinessRule,
        $"The tag name cannot exceed {Tag.MaxLength} characters.");
    
    public static readonly Error Empty = new(
        "Tag.Empty",
        ErrorType.BusinessRule,
        "The specified tag is empty.");
    
    public static readonly Error InvalidType = new(
        "Tag.InvalidType",
        ErrorType.BusinessRule,
        "The specified tag type is invalid.");
    
    public static readonly Error NoResults = new(
        "Tag.NoResults",
        ErrorType.NotFound,
        "No tags were found matching the specified criteria.");
    
    public static readonly Error NoNewTags = new(
        "Tag.NoNewTags",
        ErrorType.BusinessRule,
        "No new tags were provided to add.");
    
    public static readonly Error NoTagsToRemove = new(
        "Tag.NoTagsToRemove",
        ErrorType.BusinessRule,
        "No tags were found to remove from the image post.");
}
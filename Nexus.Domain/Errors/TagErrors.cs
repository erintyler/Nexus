using Nexus.Domain.Common;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Errors;

public static class TagErrors
{
    public static readonly Error NotFound = new(
        "Tag.NotFound",
        "The specified tag was not found.");
    
    public static readonly Error TooShort = new(
        "Tag.TooShort",
        $"The tag name must be at least {Tag.MinLength} characters long.");
    
    public static readonly Error TooLong = new(
        "Tag.TooLong",
        $"The tag name cannot exceed {Tag.MaxLength} characters.");
    
    public static readonly Error Empty = new(
        "Tag.Empty",
        "The specified tag is empty.");
    
    public static readonly Error InvalidType = new(
        "Tag.InvalidType",
        "The specified tag type is invalid.");
    
    public static readonly Error NoResults = new(
        "Tag.NoResults",
        "No tags were found matching the specified criteria.");
    
    public static readonly Error AlreadyExists = new(
        "Tag.AlreadyExists",
        "The specified tag already exists.");
}
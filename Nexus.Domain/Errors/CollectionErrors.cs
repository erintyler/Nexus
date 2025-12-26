using Nexus.Domain.Common;
using Nexus.Domain.Entities;

namespace Nexus.Domain.Errors;

public static class CollectionErrors
{
    public static readonly Error NotFound = new(
        "Collection.NotFound",
        ErrorType.NotFound,
        "The specified collection was not found.");

    public static readonly Error TitleEmpty = new(
        "Collection.Title.Empty",
        ErrorType.BusinessRule,
        "The title of the collection cannot be empty.");

    public static readonly Error TitleTooShort = new(
        "Collection.Title.TooShort",
        ErrorType.BusinessRule,
        $"Title must be at least {Collection.MinTitleLength} characters long.");

    public static readonly Error TitleTooLong = new(
        "Collection.Title.TooLong",
        ErrorType.BusinessRule,
        $"Title cannot exceed {Collection.MaxTitleLength} characters.");

    public static readonly Error UserIdEmpty = new(
        "Collection.UserId.Empty",
        ErrorType.BusinessRule,
        "User ID cannot be empty.");

    public static readonly Error ImagePostAlreadyExists = new(
        "Collection.ImagePost.AlreadyExists",
        ErrorType.BusinessRule,
        "The image post is already in this collection.");

    public static readonly Error ImagePostNotFound = new(
        "Collection.ImagePost.NotFound",
        ErrorType.BusinessRule,
        "The image post is not in this collection.");
}

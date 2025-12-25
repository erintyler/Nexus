using Nexus.Domain.Common;
using Nexus.Domain.Entities;

namespace Nexus.Domain.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new(
        "Comment.NotFound",
        ErrorType.NotFound,
        "The specified comment was not found.");

    public static readonly Error UserIdEmpty = new(
        "Comment.UserId.Empty",
        ErrorType.BusinessRule,
        "The user ID associated with the comment cannot be empty.");

    public static readonly Error ContentEmpty = new(
        "Comment.Content.Empty",
        ErrorType.BusinessRule,
        "The content of the comment cannot be empty.");

    public static readonly Error ContentTooLong = new(
        "Comment.Content.TooLong",
        ErrorType.BusinessRule,
        $"The content of the comment cannot exceed {Comment.MaxContentLength} characters.");

    public static readonly Error NotAuthor = new(
        "Comment.NotAuthor",
        ErrorType.Forbidden,
        "The user is not the author of the comment and cannot perform this action.");
}
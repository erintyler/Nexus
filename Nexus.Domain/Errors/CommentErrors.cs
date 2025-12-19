using Nexus.Domain.Common;
using Nexus.Domain.Entities;

namespace Nexus.Domain.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new(
        "Comment.NotFound",
        "The specified comment was not found.");
    
    public static readonly Error UserIdEmpty = new(
        "Comment.UserId.Empty",
        "The user ID associated with the comment cannot be empty.");

    public static readonly Error ContentEmpty = new(
        "Comment.Content.Empty",
        "The content of the comment cannot be empty.");

    public static readonly Error ContentTooLong = new(
        "Comment.Content.TooLong",
        $"The content of the comment cannot exceed {Comment.MaxContentLength} characters.");
    
    public static readonly Error NotAuthor = new(
        "Comment.NotAuthor",
        "The user is not the author of the comment and cannot perform this action.");
}
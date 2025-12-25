using AutoFixture;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Domain.UnitTests.Entities;

public class ImagePostTests
{
    private readonly Fixture _fixture = new();

    #region Create Tests

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllParametersAreValid()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(50);
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(title, result.Value.Title);
        Assert.Equal(tags.Count, result.Value.Tags.Count);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var title = _fixture.CreateString(50);
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.UserIdEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = string.Empty;
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.TitleEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsWhitespace()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = "   ";
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.TitleEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsTooLong()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(ImagePost.MaxTitleLength + 1);
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.TitleTooLong, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNoTagsProvided()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(50);
        var tags = new List<TagData>();

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.AtLeastOneTagRequired, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTagsAreInvalid()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(50);
        var tags = new List<TagData>
        {
            new(TagType.Artist, "ab") // Too short
        };

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(ImagePost.MinTitleLength)]
    [InlineData(50)]
    [InlineData(ImagePost.MaxTitleLength)]
    public void Create_ShouldReturnSuccess_WhenTitleLengthIsValid(int length)
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(length);
        var tags = _fixture.CreateTagDataList(3);

        // Act
        var result = ImagePost.Create(userId, title, tags);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region AddTags Tests

    [Fact]
    public void AddTags_ShouldReturnSuccess_WhenAddingNewTags()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(3);
        var newTags = _fixture.CreateTagDataList(2);

        // Act
        var result = imagePost.AddTags(newTags);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newTags.Count, result.Value.Count());
    }

    [Fact]
    public void AddTags_ShouldReturnFailure_WhenAllTagsAlreadyExist()
    {
        // Arrange
        var existingTags = _fixture.CreateTagDataList(3);
        var imagePost = CreateImagePostWithTags(existingTags);

        // Act
        var result = imagePost.AddTags(existingTags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(TagErrors.NoNewTags, result.Errors);
    }

    [Fact]
    public void AddTags_ShouldReturnPartialSuccess_WhenSomeTagsAlreadyExist()
    {
        // Arrange
        var existingTags = _fixture.CreateTagDataList(2);
        var imagePost = CreateImagePostWithTags(existingTags);
        var newTag = _fixture.CreateTagData();
        var tagsToAdd = existingTags.Concat(new[] { newTag }).ToList();

        // Act
        var result = imagePost.AddTags(tagsToAdd);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value); // Only the new tag should be in events
    }

    [Fact]
    public void AddTags_ShouldReturnFailure_WhenTagsAreInvalid()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var invalidTags = new List<TagData>
        {
            new(TagType.Artist, "ab") // Too short
        };

        // Act
        var result = imagePost.AddTags(invalidTags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
    }

    #endregion

    #region RemoveTags Tests

    [Fact]
    public void RemoveTags_ShouldReturnSuccess_WhenRemovingExistingTags()
    {
        // Arrange
        var existingTags = _fixture.CreateTagDataList(3);
        var imagePost = CreateImagePostWithTags(existingTags);
        var tagsToRemove = existingTags.Take(2).ToList();

        // Act
        var result = imagePost.RemoveTags(tagsToRemove);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tagsToRemove.Count, result.Value.Count());
    }

    [Fact]
    public void RemoveTags_ShouldReturnFailure_WhenNoTagsExist()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var nonExistentTags = new List<TagData>
        {
            new(TagType.Character, _fixture.CreateString(15))
        };

        // Act
        var result = imagePost.RemoveTags(nonExistentTags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(TagErrors.NoTagsToRemove, result.Errors);
    }

    [Fact]
    public void RemoveTags_ShouldReturnPartialSuccess_WhenSomeTagsExist()
    {
        // Arrange
        var existingTags = _fixture.CreateTagDataList(2);
        var imagePost = CreateImagePostWithTags(existingTags);
        var nonExistentTag = new TagData(TagType.Meta, _fixture.CreateString(15));
        var tagsToRemove = existingTags.Take(1).Concat(new[] { nonExistentTag }).ToList();

        // Act
        var result = imagePost.RemoveTags(tagsToRemove);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value); // Only the existing tag should be in events
    }

    [Fact]
    public void RemoveTags_ShouldReturnFailure_WhenTagsAreInvalid()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var invalidTags = new List<TagData>
        {
            new(TagType.Artist, "") // Empty
        };

        // Act
        var result = imagePost.RemoveTags(invalidTags);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
    }

    #endregion

    #region AddComment Tests

    [Fact]
    public void AddComment_ShouldReturnSuccess_WhenCommentIsValid()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var content = _fixture.CreateString(100);

        // Act
        var result = imagePost.AddComment(commentId, userId, content);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(commentId, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(content, result.Value.Content);
    }

    [Fact]
    public void AddComment_ShouldReturnFailure_WhenContentIsEmpty()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var content = string.Empty;

        // Act
        var result = imagePost.AddComment(commentId, userId, content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.ContentEmpty, result.Errors);
    }

    [Fact]
    public void AddComment_ShouldReturnFailure_WhenContentIsTooLong()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var content = _fixture.CreateString(Comment.MaxContentLength + 1);

        // Act
        var result = imagePost.AddComment(commentId, userId, content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.ContentTooLong, result.Errors);
    }

    [Fact]
    public void AddComment_ShouldReturnFailure_WhenUserIdIsEmpty()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = Guid.Empty;
        var content = _fixture.CreateString(100);

        // Act
        var result = imagePost.AddComment(commentId, userId, content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.UserIdEmpty, result.Errors);
    }

    #endregion

    #region UpdateComment Tests

    [Fact]
    public void UpdateComment_ShouldReturnSuccess_WhenCommentExistsAndUserIsAuthor()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);
        var newContent = _fixture.CreateString(100);

        // Act
        var result = imagePost.UpdateComment(commentId, userId, newContent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newContent, result.Value.Content);
    }

    [Fact]
    public void UpdateComment_ShouldReturnFailure_WhenCommentDoesNotExist()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var newContent = _fixture.CreateString(100);

        // Act
        var result = imagePost.UpdateComment(commentId, userId, newContent);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.NotFound, result.Errors);
    }

    [Fact]
    public void UpdateComment_ShouldReturnFailure_WhenUserIsNotAuthor()
    {
        // Arrange
        var originalUserId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(originalUserId, out var commentId);
        var differentUserId = _fixture.Create<Guid>();
        var newContent = _fixture.CreateString(100);

        // Act
        var result = imagePost.UpdateComment(commentId, differentUserId, newContent);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.NotAuthor, result.Errors);
    }

    [Fact]
    public void UpdateComment_ShouldReturnFailure_WhenNewContentIsEmpty()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);
        var newContent = string.Empty;

        // Act
        var result = imagePost.UpdateComment(commentId, userId, newContent);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.ContentEmpty, result.Errors);
    }

    [Fact]
    public void UpdateComment_ShouldReturnFailure_WhenNewContentIsTooLong()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);
        var newContent = _fixture.CreateString(Comment.MaxContentLength + 1);

        // Act
        var result = imagePost.UpdateComment(commentId, userId, newContent);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.ContentTooLong, result.Errors);
    }

    #endregion

    #region DeleteComment Tests

    [Fact]
    public void DeleteComment_ShouldReturnSuccess_WhenCommentExistsAndUserIsAuthor()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);

        // Act
        var result = imagePost.DeleteComment(commentId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentId, result.Value.Id);
    }

    [Fact]
    public void DeleteComment_ShouldReturnFailure_WhenCommentDoesNotExist()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();

        // Act
        var result = imagePost.DeleteComment(commentId, userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.NotFound, result.Errors);
    }

    [Fact]
    public void DeleteComment_ShouldReturnFailure_WhenUserIsNotAuthor()
    {
        // Arrange
        var originalUserId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(originalUserId, out var commentId);
        var differentUserId = _fixture.Create<Guid>();

        // Act
        var result = imagePost.DeleteComment(commentId, differentUserId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CommentErrors.NotAuthor, result.Errors);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void MarkAsProcessing_ShouldReturnSuccess_WhenStatusIsPending()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithTags(1, userId);

        // Act
        var result = imagePost.MarkAsProcessing(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UploadStatus.Processing, result.Value.UploadStatus);
    }

    [Fact]
    public void MarkAsProcessing_ShouldReturnFailure_WhenStatusIsNotPending()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing, userId);

        // Act
        var result = imagePost.MarkAsProcessing(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.InvalidStatusTransition, result.Errors);
    }

    [Fact]
    public void MarkAsCompleted_ShouldReturnSuccess_WhenStatusIsProcessing()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing, userId);

        // Act
        var result = imagePost.MarkAsCompleted();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UploadStatus.Completed, result.Value.UploadStatus);
    }

    [Fact]
    public void MarkAsCompleted_ShouldReturnFailure_WhenStatusIsNotProcessing()
    {
        // Arrange
        var imagePost = _fixture.CreateImagePost();

        // Act
        var result = imagePost.MarkAsCompleted();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.InvalidStatusTransition, result.Errors);
    }

    [Fact]
    public void MarkAsFailed_ShouldReturnSuccess_WhenStatusIsProcessing()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing, userId);

        // Act
        var result = imagePost.MarkAsFailed();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UploadStatus.Failed, result.Value.UploadStatus);
    }

    [Fact]
    public void MarkAsFailed_ShouldReturnFailure_WhenStatusIsNotProcessing()
    {
        // Arrange
        var imagePost = _fixture.CreateImagePost();

        // Act
        var result = imagePost.MarkAsFailed();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.InvalidStatusTransition, result.Errors);
    }

    #endregion

    #region Apply Event Tests

    [Fact]
    public void Apply_ImagePostCreatedEvent_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var imagePost = new ImagePost();
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(50);
        var tags = _fixture.CreateTagDataList(3);
        var @event = new ImagePostCreatedDomainEvent(userId, title, tags);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Equal(title, imagePost.Title);
        Assert.Equal(userId.ToString(), imagePost.CreatedBy);
        Assert.Equal(UploadStatus.Pending, imagePost.Status);
        Assert.Equal(tags.Count, imagePost.Tags.Count);
    }

    [Fact]
    public void Apply_TagAddedEvent_ShouldAddTagToCollection()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(2);
        var initialTagCount = imagePost.Tags.Count;
        var newTag = _fixture.CreateTagData();
        var @event = new TagAddedDomainEvent(newTag.Type, newTag.Value);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Equal(initialTagCount + 1, imagePost.Tags.Count);
        Assert.Contains(imagePost.Tags, t => t.Type == newTag.Type && t.Value == newTag.Value);
    }

    [Fact]
    public void Apply_TagRemovedEvent_ShouldRemoveTagFromCollection()
    {
        // Arrange
        var tags = _fixture.CreateTagDataList(3);
        var imagePost = CreateImagePostWithTags(tags);
        var tagToRemove = tags.First();
        var @event = new TagRemovedDomainEvent(tagToRemove.Type, tagToRemove.Value);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Equal(tags.Count - 1, imagePost.Tags.Count);
        Assert.DoesNotContain(imagePost.Tags, t => t.Type == tagToRemove.Type && t.Value == tagToRemove.Value);
    }

    [Fact]
    public void Apply_TagRemovedEvent_ShouldBeIdempotent_WhenTagDoesNotExist()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(2);
        var initialTagCount = imagePost.Tags.Count;
        var nonExistentTag = new TagData(TagType.Meta, _fixture.CreateString(15));
        var @event = new TagRemovedDomainEvent(nonExistentTag.Type, nonExistentTag.Value);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Equal(initialTagCount, imagePost.Tags.Count); // No change
    }

    [Fact]
    public void Apply_TagMigratedEvent_ShouldReplaceSourceTagWithTargetTag()
    {
        // Arrange
        var sourceTag = _fixture.CreateTagData();
        var imagePost = CreateImagePostWithTags(new[] { sourceTag });
        var targetTag = new TagData(TagType.Character, _fixture.CreateString(15));
        var userId = _fixture.Create<Guid>();
        var @event = new TagMigratedDomainEvent(userId, sourceTag, targetTag);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Single(imagePost.Tags);
        Assert.DoesNotContain(imagePost.Tags, t => t.Type == sourceTag.Type && t.Value == sourceTag.Value);
        Assert.Contains(imagePost.Tags, t => t.Type == targetTag.Type && t.Value == targetTag.Value);
    }

    [Fact]
    public void Apply_CommentCreatedEvent_ShouldAddCommentToCollection()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var commentId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var content = _fixture.CreateString(100);
        var @event = new CommentCreatedDomainEvent(commentId, userId, content);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Single(imagePost.Comments);
        var comment = imagePost.Comments.First();
        Assert.Equal(commentId, comment.Id);
        Assert.Equal(userId, comment.UserId);
        Assert.Equal(content, comment.Content);
    }

    [Fact]
    public void Apply_CommentUpdatedEvent_ShouldUpdateCommentContent()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);
        var newContent = _fixture.CreateString(100);
        var @event = new CommentUpdatedDomainEvent(commentId, userId, newContent);

        // Act
        imagePost.Apply(@event);

        // Assert
        var comment = imagePost.Comments.First(c => c.Id == commentId);
        Assert.Equal(newContent, comment.Content);
    }

    [Fact]
    public void Apply_CommentDeletedEvent_ShouldRemoveCommentFromCollection()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var imagePost = CreateImagePostWithComment(userId, out var commentId);

        // Add second comment to verify only the target is removed
        var secondCommentId = _fixture.Create<Guid>();
        imagePost.Apply(new CommentCreatedDomainEvent(secondCommentId, userId, _fixture.CreateString(100)));

        var @event = new CommentDeletedDomainEvent(commentId, userId);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Single(imagePost.Comments);
        Assert.DoesNotContain(imagePost.Comments, c => c.Id == commentId);
        Assert.Contains(imagePost.Comments, c => c.Id == secondCommentId);
    }

    [Fact]
    public void Apply_StatusChangedEvent_ShouldUpdateStatus()
    {
        // Arrange
        var imagePost = CreateImagePostWithTags(1);
        var userId = _fixture.Create<Guid>();
        var @event = new StatusChangedDomainEvent(imagePost.Id, UploadStatus.Processing, userId);

        // Act
        imagePost.Apply(@event);

        // Assert
        Assert.Equal(UploadStatus.Processing, imagePost.Status);
    }

    #endregion

    #region Helper Methods

    private ImagePost CreateImagePostWithTags(int tagCount, Guid? userId = null)
    {
        var tags = _fixture.CreateTagDataList(tagCount);
        return _fixture.CreateImagePost(userId, tags: tags);
    }

    private ImagePost CreateImagePostWithTags(IEnumerable<TagData> tags, Guid? userId = null)
    {
        return _fixture.CreateImagePost(userId, tags: tags.ToList());
    }

    private ImagePost CreateImagePostWithComment(Guid userId, out Guid commentId)
    {
        var imagePost = _fixture.CreateImagePost();
        commentId = _fixture.Create<Guid>();
        var content = _fixture.CreateString(100);
        var commentEvent = new CommentCreatedDomainEvent(commentId, userId, content);

        imagePost.Apply(commentEvent);
        return imagePost;
    }

    #endregion
}


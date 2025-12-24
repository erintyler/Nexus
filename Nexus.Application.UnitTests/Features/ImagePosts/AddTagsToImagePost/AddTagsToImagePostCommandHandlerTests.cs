using AutoFixture;
using Moq;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.AddTagsToImagePost;
using Nexus.UnitTests.Utilities.Extensions;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;

namespace Nexus.Application.UnitTests.Features.ImagePosts.AddTagsToImagePost;

public class AddTagsToImagePostCommandHandlerTests
{
    private readonly Mock<ITagMigrationService> _mockTagMigrationService = new();
    private readonly Fixture _fixture = new();
    
    [Fact]
    public async Task HandleAsync_ShouldAddTagsToImage_WhenTagsAreValid()
    {
        // Arrange
        var imagePost = new ImagePost();
        var command = _fixture.Build<AddTagsToImagePostCommand>()
            .With(x => x.Tags, _fixture.CreateTagDtoList()
                .ToList())
            .Create();
        
        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);
        
        // Act
        var (result, events) = await AddTagsToImagePostCommandHandler.HandleAsync(
            command,
            imagePost,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(events);
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.SequenceEqual(expectedTagData)),
            It.IsAny<CancellationToken>()), Times.Once);
        
        var tagAddedEvents = events.OfType<TagAddedDomainEvent>().ToList();
        Assert.Equal(expectedTagData.Count, tagAddedEvents.Count);
        
        foreach (var expectedTag in expectedTagData)
        {
            Assert.Contains(tagAddedEvents, e => 
                e.TagType == expectedTag.Type && 
                e.TagValue == expectedTag.Value);
        }
    }
    
    [Fact]
    public async Task HandleAsync_ShouldHandleTagMigrationsCorrectly()
    {
        // Arrange
        var imagePost = new ImagePost();
        var command = _fixture.Build<AddTagsToImagePostCommand>()
            .With(x => x.Tags, _fixture.CreateTagDtoList()
                .ToList())
            .Create();
        
        var migratedTagData = command.Tags
            .Select(t => new TagData(t.Type, _fixture.CreateString(30)))
            .ToList();
        
        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(migratedTagData);
        
        // Act
        var (result, events) = await AddTagsToImagePostCommandHandler.HandleAsync(
            command,
            imagePost,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(events);
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.SequenceEqual(
                command.Tags.Select(t => new TagData(t.Type, t.Value)))),
            It.IsAny<CancellationToken>()), Times.Once);
        
        var tagAddedEvents = events.OfType<TagAddedDomainEvent>().ToList();
        Assert.Equal(migratedTagData.Count, tagAddedEvents.Count);
        
        foreach (var migratedTag in migratedTagData)
        {
            Assert.Contains(tagAddedEvents, e => 
                e.TagType == migratedTag.Type && 
                e.TagValue == migratedTag.Value);
        }
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCreateEvents_ForDuplicateTags()
    {
        // Arrange
        var imagePost = new ImagePost();
        var existingTagEvent = _fixture.Build<TagAddedDomainEvent>()
            .With(x => x.TagValue, _fixture.CreateString(30))
            .Create();
        
        // Manually apply the existing tag to the image post
        imagePost.Apply(existingTagEvent);
        
        var command = new AddTagsToImagePostCommand(
            imagePost.Id,
            new List<TagDto>
            {
                new(existingTagEvent.TagType, existingTagEvent.TagValue), // Duplicate tag
                _fixture.CreateTagDto() // New tag
            });
        
        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);
        
        // Act
        var (result, events) = await AddTagsToImagePostCommandHandler.HandleAsync(
            command,
            imagePost,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        var tagAddedEvents = events.OfType<TagAddedDomainEvent>().ToList();
        Assert.Single(tagAddedEvents); // Only one new tag should be added
        
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.SequenceEqual(expectedTagData)),
            It.IsAny<CancellationToken>()), Times.Once);
        
        var newTag = expectedTagData.Last();
        Assert.Equal(newTag.Type, tagAddedEvents[0].TagType);
        Assert.Equal(newTag.Value, tagAddedEvents[0].TagValue);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenNoNewTagsAreProvided()
    {
        // Arrange
        var imagePost = new ImagePost();
        var existingTagEvent = _fixture.Build<TagAddedDomainEvent>()
            .With(x => x.TagValue, _fixture.CreateString(30))
            .Create();
        
        // Manually apply the existing tag to the image post
        imagePost.Apply(existingTagEvent);
        
        var command = new AddTagsToImagePostCommand(
            imagePost.Id,
            new List<TagDto>
            {
                new(existingTagEvent.TagType, existingTagEvent.TagValue) // Duplicate tag
            });
        
        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);
        
        // Act
        var (result, events) = await AddTagsToImagePostCommandHandler.HandleAsync(
            command,
            imagePost,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(result.Errors[0], TagErrors.NoNewTags);
        Assert.Empty(events); // No new events should be created
        
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.SequenceEqual(expectedTagData)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
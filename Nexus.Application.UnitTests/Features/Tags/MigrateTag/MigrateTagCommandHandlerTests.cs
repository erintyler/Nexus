using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.UnitTests.Utilities.Extensions;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;

namespace Nexus.Application.UnitTests.Features.Tags.MigrateTag;

public class MigrateTagCommandHandlerTests
{
    private readonly Mock<ITagMigrationRepository> _mockRepository = new();
    private readonly Mock<IUserContextService> _mockUserContextService = new();
    private readonly Mock<ILogger<MigrateTagCommandHandler>> _mockLogger = new();
    private readonly Fixture _fixture = new();
    private readonly MigrateTagCommandHandler _handler;

    public MigrateTagCommandHandlerTests()
    {
        _handler = new MigrateTagCommandHandler(
            _mockRepository.Object,
            _mockUserContextService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldMigrateTagSuccessfully_WhenNoExistingMigrationExists()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration>());

        var posts = _fixture.CreateMany<ImagePostReadModel>(3).ToList();

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(posts.ToAsyncEnumerable());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.PostsMigrated);
        Assert.Equal(0, result.Value.UpstreamMigrationsUpdated);
        Assert.True(result.Value.Success);
        Assert.Equal("Tag migration completed successfully", result.Value.Message);

        _mockRepository.Verify(r => r.CreateMigrationAsync(
            It.Is<TagMigration>(m => 
                m.SourceTag.Type == command.Source.Type &&
                m.SourceTag.Value == command.Source.Value &&
                m.TargetTag.Type == command.Target.Type &&
                m.TargetTag.Value == command.Target.Value),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenMigrationAlreadyExists()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var existingMigration = TagMigration.Create(
            userId,
            new TagData(command.Source.Type, command.Source.Value),
            new TagData(TagType.Character, "some-other-target")).Value;

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMigration);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == TagMigrationErrors.AlreadyExists.Code);

        _mockRepository.Verify(r => r.CreateMigrationAsync(
            It.IsAny<TagMigration>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateUpstreamMigrations_WhenTheyExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        // Create upstream migrations that point to the source tag as their target
        var upstreamUserId = _fixture.Create<Guid>();
        var upstreamMigration1 = TagMigration.Create(
            upstreamUserId,
            new TagData(TagType.Artist, "old-artist-1"),
            new TagData(command.Source.Type, command.Source.Value)).Value;

        var upstreamMigration2 = TagMigration.Create(
            upstreamUserId,
            new TagData(TagType.Artist, "old-artist-2"),
            new TagData(command.Source.Type, command.Source.Value)).Value;

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.Is<TagData>(t => t.Type == command.Source.Type && t.Value == command.Source.Value),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration> { upstreamMigration1, upstreamMigration2 });

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<ImagePostReadModel>());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.UpstreamMigrationsUpdated);
        Assert.Equal(0, result.Value.PostsMigrated);

        _mockRepository.Verify(r => r.UpdateMigrationsAsync(
            It.Is<List<TagMigration>>(list => list.Count == 2),
            It.Is<List<TagMigration>>(list => list.Count == 2 && 
                list.All(m => m.TargetTag.Type == command.Target.Type && 
                            m.TargetTag.Value == command.Target.Value)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldProcessPostsInBatches_WhenManyPostsExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration>());

        // Create 750 posts (should result in 2 batches: 500 + 250)
        var posts = _fixture.CreateMany<ImagePostReadModel>(750).ToList();

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(posts.ToAsyncEnumerable());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(750, result.Value.PostsMigrated);

        // Verify that AppendMigrationEventsBatchAsync was called twice (for 2 batches)
        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify the batch sizes: first batch should be 500, second should be 250
        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.Is<List<Guid>>(list => list.Count == 500),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.Is<List<Guid>>(list => list.Count == 250),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateMigrationEventWithCorrectData()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration>());

        var posts = _fixture.CreateMany<ImagePostReadModel>(1).ToList();

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(posts.ToAsyncEnumerable());

        TagMigratedDomainEvent? capturedEvent = null;
        _mockRepository
            .Setup(r => r.AppendMigrationEventsBatchAsync(
                It.IsAny<List<Guid>>(),
                It.IsAny<TagMigratedDomainEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<Guid>, TagMigratedDomainEvent, CancellationToken>((ids, evt, ct) =>
            {
                capturedEvent = evt;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedEvent);
        Assert.Equal(userId, capturedEvent.UserId);
        Assert.Equal(command.Source.Type, capturedEvent.Source.Type);
        Assert.Equal(command.Source.Value, capturedEvent.Source.Value);
        Assert.Equal(command.Target.Type, capturedEvent.Target.Type);
        Assert.Equal(command.Target.Value, capturedEvent.Target.Value);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNoPosts_Successfully()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration>());

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<ImagePostReadModel>());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.PostsMigrated);
        Assert.Equal(0, result.Value.UpstreamMigrationsUpdated);

        _mockRepository.Verify(r => r.CreateMigrationAsync(
            It.IsAny<TagMigration>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectSourceTag()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var sourceTag = _fixture.CreateTagDto();
        var targetTag = _fixture.CreateTagDto();
        var command = new MigrateTagCommand(Source: sourceTag, Target: targetTag);

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration>());

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<ImagePostReadModel>());

        // Act
        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.GetMigrationBySourceAsync(
            It.Is<TagData>(t => t.Type == sourceTag.Type && t.Value == sourceTag.Value),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.GetPostsWithTagAsync(
            It.Is<TagData>(t => t.Type == sourceTag.Type && t.Value == sourceTag.Value),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.GetUpstreamMigrationsAsync(
            It.Is<TagData>(t => t.Type == sourceTag.Type && t.Value == sourceTag.Value),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldContinueProcessing_WhenUpstreamMigrationUpdateFails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        // Create an invalid upstream migration that will fail to update
        var invalidMigration = new TagMigration
        {
            CreatedBy = Guid.Empty.ToString(),
            SourceTag = new TagData(TagType.Artist, "old-artist"),
            TargetTag = new TagData(command.Source.Type, command.Source.Value)
        };

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([invalidMigration]);

        var posts = _fixture.CreateMany<ImagePostReadModel>(1).ToList();

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(posts.ToAsyncEnumerable());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PostsMigrated);
        Assert.Equal(0, result.Value.UpstreamMigrationsUpdated); // Should be 0 because update failed
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleBothUpstreamMigrationsAndPosts()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var command = new MigrateTagCommand(
            Source: _fixture.CreateTagDto(),
            Target: _fixture.CreateTagDto());

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetMigrationBySourceAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagMigration?)null);

        // Setup upstream migrations
        var upstreamUserId = _fixture.Create<Guid>();
        var upstreamMigration = TagMigration.Create(
            upstreamUserId,
            new TagData(TagType.Character, "old-character"),
            new TagData(command.Source.Type, command.Source.Value)).Value;

        _mockRepository
            .Setup(r => r.GetUpstreamMigrationsAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagMigration> { upstreamMigration });

        // Setup posts
        var posts = _fixture.CreateMany<ImagePostReadModel>(2).ToList();

        _mockRepository
            .Setup(r => r.GetPostsWithTagAsync(
                It.IsAny<TagData>(),
                It.IsAny<CancellationToken>()))
            .Returns(posts.ToAsyncEnumerable());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PostsMigrated);
        Assert.Equal(1, result.Value.UpstreamMigrationsUpdated);

        _mockRepository.Verify(r => r.CreateMigrationAsync(
            It.IsAny<TagMigration>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.UpdateMigrationsAsync(
            It.IsAny<List<TagMigration>>(),
            It.IsAny<List<TagMigration>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(r => r.AppendMigrationEventsBatchAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<TagMigratedDomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}


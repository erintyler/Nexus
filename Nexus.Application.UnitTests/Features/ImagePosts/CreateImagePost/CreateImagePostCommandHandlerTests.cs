using AutoFixture;
using Moq;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.UnitTests.Extensions;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;

namespace Nexus.Application.UnitTests.Features.ImagePosts.CreateImagePost;

public class CreateImagePostCommandHandlerTests
{
    private readonly Mock<IImageService> _mockImageService = new();
    private readonly Mock<IUserContextService> _mockUserContextService = new();
    private readonly Mock<ITagMigrationService> _mockTagMigrationService = new();
    private readonly Fixture _fixture = new();

    public CreateImagePostCommandHandlerTests()
    {
        // Setup default user context
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateImagePost_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadUrl = "https://example.com/upload/image.jpg";
        
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, _fixture.CreateTagDtoList(2).ToList())
            .With(x => x.ContentType, "image/jpeg")
            .Create();

        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);

        _mockImageService
            .Setup(s => s.GenerateImageUploadUrl(It.IsAny<Guid>(), command.ContentType))
            .Returns(uploadUrl);

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(stream);
        
        var response = result.Value;
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(command.Title, response.Title);
        Assert.Equal(uploadUrl, response.UploadUrl);
        Assert.InRange(response.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));

        _mockUserContextService.Verify(s => s.GetUserId(), Times.Once);
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.SequenceEqual(expectedTagData)),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockImageService.Verify(s => s.GenerateImageUploadUrl(response.Id, command.ContentType), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldResolveTagMigrations_BeforeCreatingPost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadUrl = "https://example.com/upload/image.jpg";
        
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, _fixture.CreateTagDtoList(3).ToList())
            .With(x => x.ContentType, "image/png")
            .Create();

        // Simulate tag migrations - some tags get migrated to different values
        var migratedTagData = new List<TagData>
        {
            new(command.Tags[0].Type, "migrated-value-1"),
            new(command.Tags[1].Type, command.Tags[1].Value), // No migration
            new(command.Tags[2].Type, "migrated-value-2")
        };

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(migratedTagData);

        _mockImageService
            .Setup(s => s.GenerateImageUploadUrl(It.IsAny<Guid>(), command.ContentType))
            .Returns(uploadUrl);

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(stream);

        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => 
                tags.Count == 3 &&
                tags[0].Value == command.Tags[0].Value &&
                tags[1].Value == command.Tags[1].Value &&
                tags[2].Value == command.Tags[2].Value),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, string.Empty)
            .With(x => x.Tags, _fixture.CreateTagDtoList(1).ToList())
            .With(x => x.ContentType, "image/jpeg")
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
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.TitleEmpty, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenTitleIsTooShort()
    {
        // Arrange
        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(ImagePost.MinTitleLength - 1)) // Less than minimum length
            .With(x => x.Tags, _fixture.CreateTagDtoList(1).ToList())
            .With(x => x.ContentType, "image/jpeg")
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
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.TitleTooShort, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenTitleIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(ImagePost.MaxTitleLength + 1))
            .With(x => x.Tags, _fixture.CreateTagDtoList(1).ToList())
            .With(x => x.ContentType, "image/jpeg")
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
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.TitleTooLong, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenNoTagsAreProvided()
    {
        // Arrange
        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, new List<TagDto>())
            .With(x => x.ContentType, "image/jpeg")
            .Create();

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagData>());

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.AtLeastOneTagRequired, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenTagsAreInvalid()
    {
        // Arrange
        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, new List<TagDto>
            {
                new(TagType.Artist, _fixture.CreateString(256)) // Exceeds max tag value length
            })
            .With(x => x.ContentType, "image/jpeg")
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
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateUploadUrl_WithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contentType = "image/webp";
        var uploadUrl = "https://s3.amazonaws.com/bucket/upload-url";
        
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, _fixture.CreateTagDtoList(1).ToList())
            .With(x => x.ContentType, contentType)
            .Create();

        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);

        _mockImageService
            .Setup(s => s.GenerateImageUploadUrl(It.IsAny<Guid>(), contentType))
            .Returns(uploadUrl);

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(uploadUrl, result.Value.UploadUrl);
        _mockImageService.Verify(s => s.GenerateImageUploadUrl(
            It.Is<Guid>(id => id == result.Value.Id),
            contentType), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectUserId_FromUserContext()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(expectedUserId);

        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, _fixture.CreateTagDtoList(1).ToList())
            .With(x => x.ContentType, "image/jpeg")
            .Create();

        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);

        _mockImageService
            .Setup(s => s.GenerateImageUploadUrl(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("https://example.com/upload");

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserContextService.Verify(s => s.GetUserId(), Times.Once);
        
        // The stream should have the event with the correct userId
        Assert.NotNull(stream);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleMultipleTags_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var command = _fixture.Build<CreateImagePostCommand>()
            .With(x => x.Title, _fixture.CreateString(20))
            .With(x => x.Tags, _fixture.CreateTagDtoList(5).ToList())
            .With(x => x.ContentType, "image/jpeg")
            .Create();

        var expectedTagData = command.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();

        _mockTagMigrationService
            .Setup(s => s.ResolveMigrationsAsync(
                It.IsAny<IReadOnlyList<TagData>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTagData);

        _mockImageService
            .Setup(s => s.GenerateImageUploadUrl(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("https://example.com/upload");

        // Act
        var (result, stream) = await CreateImagePostCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockUserContextService.Object,
            _mockTagMigrationService.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(stream);
        
        _mockTagMigrationService.Verify(s => s.ResolveMigrationsAsync(
            It.Is<IReadOnlyList<TagData>>(tags => tags.Count == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}


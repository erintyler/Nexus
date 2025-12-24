using AutoFixture;
using Moq;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;

namespace Nexus.Application.UnitTests.Features.ImagePosts.GetImageById;

public class GetImagePostQueryHandlerTests
{
    private readonly Mock<IImageService> _mockImageService = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task HandleAsync_ShouldReturnImagePostDto_WhenImagePostExistsAndIsCompleted()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        var userId = _fixture.Create<string>();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var lastModified = DateTimeOffset.UtcNow;

        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = createdAt,
            LastModified = lastModified,
            CreatedBy = userId,
            LastModifiedBy = userId,
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.Artist),
                new(_fixture.Create<string>(), TagType.Character),
                new(_fixture.Create<string>(), TagType.General)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        Assert.Equal(imagePost.Title, dto.Title);
        Assert.Equal(processedImageUrl, dto.Url);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(lastModified, dto.LastUpdated);
        Assert.Equal(userId, dto.CreatedBy);
        Assert.Equal(userId, dto.LastUpdatedBy);
        Assert.Equal(3, dto.Tags.Count);
        
        Assert.Single(dto.Tags, t => t.Type == TagType.Artist);
        Assert.Single(dto.Tags, t => t.Type == TagType.Character);
        Assert.Single(dto.Tags, t => t.Type == TagType.General);

        _mockImageService.Verify(s => s.GetProcessedImageUrl(imagePostId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenImagePostIsNull()
    {
        // Arrange
        var query = new GetImagePostQuery(Guid.NewGuid());
        ImagePostReadModel? imagePost = null;

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
        
        _mockImageService.Verify(s => s.GetProcessedImageUrl(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenImagePostStatusIsPending()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.General)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
        
        _mockImageService.Verify(s => s.GetProcessedImageUrl(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenImagePostStatusIsProcessing()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.Artist)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
        
        _mockImageService.Verify(s => s.GetProcessedImageUrl(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenImagePostStatusIsFailed()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Failed,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.Meta)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
        
        _mockImageService.Verify(s => s.GetProcessedImageUrl(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyTagsList_WhenImagePostHasNoTags()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>()
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        Assert.Empty(dto.Tags);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllTagTypes_WhenImagePostHasMultipleTagTypes()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.Artist),
                new(_fixture.Create<string>(), TagType.Series),
                new(_fixture.Create<string>(), TagType.Character),
                new(_fixture.Create<string>(), TagType.General),
                new(_fixture.Create<string>(), TagType.Meta)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        Assert.Equal(5, dto.Tags.Count);
        
        Assert.Single(dto.Tags, t => t.Type == TagType.Artist);
        Assert.Single(dto.Tags, t => t.Type == TagType.Series);
        Assert.Single(dto.Tags, t => t.Type == TagType.Character);
        Assert.Single(dto.Tags, t => t.Type == TagType.General);
        Assert.Single(dto.Tags, t => t.Type == TagType.Meta);
    }

    [Fact]
    public async Task HandleAsync_ShouldPreserveTagOrder_WhenMappingTags()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        var firstTag = _fixture.Create<string>();
        var secondTag = _fixture.Create<string>();
        var thirdTag = _fixture.Create<string>();
        
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(firstTag, TagType.General),
                new(secondTag, TagType.Artist),
                new(thirdTag, TagType.Character)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        Assert.Equal(3, dto.Tags.Count);
        
        Assert.Equal(TagType.General, dto.Tags[0].Type);
        Assert.Equal(firstTag, dto.Tags[0].Value);
        
        Assert.Equal(TagType.Artist, dto.Tags[1].Type);
        Assert.Equal(secondTag, dto.Tags[1].Value);
        
        Assert.Equal(TagType.Character, dto.Tags[2].Type);
        Assert.Equal(thirdTag, dto.Tags[2].Value);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectImagePostId_WhenGeneratingProcessedImageUrl()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = _fixture.Create<string>(),
            LastModifiedBy = _fixture.Create<string>(),
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.General)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(processedImageUrl, result.Value.Url);
        
        _mockImageService.Verify(s => s.GetProcessedImageUrl(imagePostId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapDifferentCreatorAndModifier_WhenTheyAreDifferent()
    {
        // Arrange
        var imagePostId = Guid.NewGuid();
        var processedImageUrl = _fixture.Create<string>();
        var creator = _fixture.Create<string>();
        var modifier = _fixture.Create<string>();
        
        var imagePost = new ImagePostReadModel
        {
            Id = imagePostId,
            Title = _fixture.Create<string>(),
            Status = UploadStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = creator,
            LastModifiedBy = modifier,
            Tags = new List<TagReadModel>
            {
                new(_fixture.Create<string>(), TagType.General)
            }
        };

        var query = new GetImagePostQuery(imagePostId);

        _mockImageService
            .Setup(s => s.GetProcessedImageUrl(imagePostId))
            .Returns(processedImageUrl);

        // Act
        var result = await GetImagePostQueryHandler.HandleAsync(
            query,
            _mockImageService.Object,
            imagePost);

        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        Assert.Equal(creator, dto.CreatedBy);
        Assert.Equal(modifier, dto.LastUpdatedBy);
        Assert.NotEqual(dto.CreatedBy, dto.LastUpdatedBy);
    }
}


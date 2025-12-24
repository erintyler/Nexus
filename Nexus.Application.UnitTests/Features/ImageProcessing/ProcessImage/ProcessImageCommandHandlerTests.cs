using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.ImageProcessing.ProcessImage;

public class ProcessImageCommandHandlerTests
{
    private readonly Mock<IImageService> _mockImageService = new();
    private readonly Mock<IImageConversionService> _mockImageConversionService = new();
    private readonly Mock<IThumbnailService> _mockThumbnailService = new();
    private readonly Mock<ILogger<ProcessImageCommandHandler>> _mockLogger = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task HandleAsync_ShouldProcessImageSuccessfully_WhenAllOperationsSucceed()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageService.Verify(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Once);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Once);
        _mockImageService.Verify(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageService.Verify(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailedEvent_WhenOriginalImageNotFound()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageService.Verify(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Never);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Never);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Original image not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailedEvent_WhenWebPConversionFails()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var conversionError = ImageProcessingErrors.EncodeFailed;
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(conversionError));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageService.Verify(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Once);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Once);
        _mockImageService.Verify(s => s.SaveProcessedImageAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockImageService.Verify(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image processing failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailedEvent_WhenThumbnailCreationFails()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailError = ImageProcessingErrors.EncodeFailed;

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(thumbnailError));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageService.Verify(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Once);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Once);
        _mockImageService.Verify(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageService.Verify(s => s.SaveThumbnailAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image processing failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailedEvent_WhenBothWebPAndThumbnailFail()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var conversionError = ImageProcessingErrors.EncodeFailed;
        var thumbnailError = ImageProcessingErrors.EncodeFailed;

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(conversionError));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(thumbnailError));

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageService.Verify(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Once);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Once);
        _mockImageService.Verify(s => s.SaveProcessedImageAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockImageService.Verify(s => s.SaveThumbnailAsync(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image processing failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldProcessBothTasksConcurrently_WhenCalled()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        _mockImageConversionService.Verify(s => s.ConvertToWebP(It.IsAny<Stream>()), Times.Once);
        _mockThumbnailService.Verify(s => s.CreateThumbnail(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReadImageDataCorrectly_WhenOriginalImageIsRetrieved()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var expectedImageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(expectedImageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        byte[]? capturedWebPInputData = null;
        byte[]? capturedThumbnailInputData = null;

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns((Stream stream) =>
            {
                capturedWebPInputData = ((MemoryStream)stream).ToArray();
                return Result.Success(webpData);
            });

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns((Stream stream) =>
            {
                capturedThumbnailInputData = ((MemoryStream)stream).ToArray();
                return Result.Success(thumbnailData);
            });

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(capturedWebPInputData);
        Assert.NotNull(capturedThumbnailInputData);
        Assert.Equal(expectedImageData, capturedWebPInputData);
        Assert.Equal(expectedImageData, capturedThumbnailInputData);
    }

    [Fact]
    public async Task HandleAsync_ShouldDisposeSteams_AfterProcessing()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        // Verify the original stream was disposed by attempting to read from it
        Assert.Throws<ObjectDisposedException>(() => originalImageStream.ReadByte());
    }

    [Fact]
    public async Task HandleAsync_ShouldLogCorrectImageId_InAllLogMessages()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Processing);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(imageId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogError_WhenMarkAsFailedFails()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        // Create ImagePost with Completed status, so MarkAsFailed will fail
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Completed);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var conversionError = ImageProcessingErrors.EncodeFailed;

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(conversionError));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Failure<byte[]>(conversionError));

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
        
        // Verify that error was logged for image processing failed
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image processing failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        // Verify that error was logged when MarkAsFailed itself failed
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to mark ImagePost as failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogError_WhenMarkAsCompletedFails()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var command = new ProcessImageCommand(imageId);
        // Create ImagePost with Completed status, so MarkAsCompleted will fail
        var imagePost = _fixture.CreateImagePostWithStatus(UploadStatus.Completed);
        
        var imageData = _fixture.CreateMany<byte>(1024).ToArray();
        var originalImageStream = new MemoryStream(imageData);
        var webpData = _fixture.CreateMany<byte>(512).ToArray();
        var thumbnailData = _fixture.CreateMany<byte>(256).ToArray();

        _mockImageService
            .Setup(s => s.GetOriginalImageStreamAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalImageStream);

        _mockImageConversionService
            .Setup(s => s.ConvertToWebP(It.IsAny<Stream>()))
            .Returns(Result.Success(webpData));

        _mockThumbnailService
            .Setup(s => s.CreateThumbnail(It.IsAny<Stream>()))
            .Returns(Result.Success(thumbnailData));

        _mockImageService
            .Setup(s => s.SaveProcessedImageAsync(imageId, webpData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockImageService
            .Setup(s => s.SaveThumbnailAsync(imageId, thumbnailData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await ProcessImageCommandHandler.HandleAsync(
            command,
            _mockImageService.Object,
            _mockImageConversionService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object,
            imagePost,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
        
        // Verify that success was logged for image processing
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        // Verify that error was logged when MarkAsCompleted itself failed
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to mark ImagePost as completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}


using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.UnitTests.Utilities.Helpers;
using SkiaSharp;

namespace Nexus.Application.UnitTests.Features.ImageProcessing.ProcessImage;

public class ThumbnailServiceTests
{
    private readonly ThumbnailService _service = new();

    [Fact]
    public void CreateThumbnail_ShouldReturnSuccess_WhenValidJpegImageProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage();

        // Act
        var result = _service.CreateThumbnail(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify it's a valid WebP thumbnail
        using var verifyStream = new MemoryStream(result.Value);
        using var verifyBitmap = SKBitmap.Decode(verifyStream);
        Assert.NotNull(verifyBitmap);

        // Verify it's actually resized
        Assert.True(verifyBitmap.Width <= ThumbnailService.ThumbnailWidth);
        Assert.True(verifyBitmap.Height <= ThumbnailService.ThumbnailHeight);
    }

    [Fact]
    public void CreateThumbnail_ShouldReturnSuccess_WhenValidPngImageProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(1024, 768, SKEncodedImageFormat.Png);

        // Act
        var result = _service.CreateThumbnail(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public void CreateThumbnail_ShouldReturnDecodeFailed_WhenInvalidImageProvided()
    {
        // Arrange
        using var invalidStream = TestImageHelper.CreateInvalidImageStream();

        // Act
        var result = _service.CreateThumbnail(invalidStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void CreateThumbnail_ShouldReturnDecodeFailed_WhenEmptyStreamProvided()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var result = _service.CreateThumbnail(emptyStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void CreateThumbnail_ShouldReturnDecodeFailed_WhenNonImageDataProvided()
    {
        // Arrange
        var textData = System.Text.Encoding.UTF8.GetBytes("This is not an image");
        using var textStream = new MemoryStream(textData);

        // Act
        var result = _service.CreateThumbnail(textStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void CreateThumbnail_ShouldProduceWebPImage_WhenValidImageProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage();

        // Act
        var result = _service.CreateThumbnail(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the output is WebP format
        using var outputStream = new MemoryStream(result.Value);
        using var codec = SKCodec.Create(outputStream);
        Assert.NotNull(codec);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
    }

    [Fact]
    public void CreateThumbnail_ShouldResizeLandscapeImage_MaintainingAspectRatio()
    {
        // Arrange
        using var landscapeStream = TestImageHelper.CreateTestImage(1920, 1080);
        var originalAspectRatio = 1920.0 / 1080.0;

        // Act
        var result = _service.CreateThumbnail(landscapeStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);

        Assert.NotNull(thumbnailBitmap);
        Assert.Equal(ThumbnailService.ThumbnailWidth, thumbnailBitmap.Width);

        var thumbnailAspectRatio = (double)thumbnailBitmap.Width / thumbnailBitmap.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 0.01);
    }

    [Fact]
    public void CreateThumbnail_ShouldResizePortraitImage_MaintainingAspectRatio()
    {
        // Arrange
        using var portraitStream = TestImageHelper.CreateTestImage(600, 800);
        var originalAspectRatio = 600.0 / 800.0;

        // Act
        var result = _service.CreateThumbnail(portraitStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);

        Assert.NotNull(thumbnailBitmap);
        Assert.Equal(ThumbnailService.ThumbnailHeight, thumbnailBitmap.Height);

        var thumbnailAspectRatio = (double)thumbnailBitmap.Width / thumbnailBitmap.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 0.01);
    }

    [Fact]
    public void CreateThumbnail_ShouldResizeSquareImage_ToMaxDimension()
    {
        // Arrange
        using var squareStream = TestImageHelper.CreateTestImage(1000, 1000);

        // Act
        var result = _service.CreateThumbnail(squareStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);

        Assert.NotNull(thumbnailBitmap);
        Assert.Equal(ThumbnailService.ThumbnailHeight, thumbnailBitmap.Width);
        Assert.Equal(ThumbnailService.ThumbnailHeight, thumbnailBitmap.Height);
    }

    [Fact]
    public void CreateThumbnail_ShouldReduceFileSize_SignificantlyFromOriginal()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(4000, 3000);
        var originalSize = imageStream.Length;

        // Act
        var result = _service.CreateThumbnail(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Length < originalSize,
            $"Thumbnail size ({result.Value.Length}) should be smaller than original size ({originalSize})");
    }

    [Fact]
    public void CreateThumbnail_ShouldHandleVeryLargeImage_Successfully()
    {
        // Arrange
        using var largeImageStream = TestImageHelper.CreateTestImage(5000, 4000);

        // Act
        var result = _service.CreateThumbnail(largeImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify the thumbnail respects maximum dimensions
        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);
        Assert.True(thumbnailBitmap.Width <= ThumbnailService.ThumbnailWidth);
        Assert.True(thumbnailBitmap.Height <= ThumbnailService.ThumbnailHeight);
    }

    [Fact]
    public void CreateThumbnail_ShouldHandleAlreadySmallImage_Successfully()
    {
        // Arrange
        using var smallImageStream = TestImageHelper.CreateTestImage(300, 200);

        // Act
        var result = _service.CreateThumbnail(smallImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Even small images should be resized to fit thumbnail dimensions
        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);
        Assert.NotNull(thumbnailBitmap);
    }

    [Fact]
    public void CreateThumbnail_ShouldHandleExtremeAspectRatio_Landscape()
    {
        // Arrange - very wide image
        using var wideImageStream = TestImageHelper.CreateTestImage(3000, 500);
        var originalAspectRatio = 3000.0 / 500.0;

        // Act
        var result = _service.CreateThumbnail(wideImageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);

        Assert.NotNull(thumbnailBitmap);
        var thumbnailAspectRatio = (double)thumbnailBitmap.Width / thumbnailBitmap.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 0.05);
    }

    [Fact]
    public void CreateThumbnail_ShouldHandleExtremeAspectRatio_Portrait()
    {
        // Arrange - very tall image
        using var tallImageStream = TestImageHelper.CreateTestImage(500, 3000);
        var originalAspectRatio = 500.0 / 3000.0;

        // Act
        var result = _service.CreateThumbnail(tallImageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);

        Assert.NotNull(thumbnailBitmap);
        var thumbnailAspectRatio = (double)thumbnailBitmap.Width / thumbnailBitmap.Height;
        Assert.Equal(originalAspectRatio, thumbnailAspectRatio, 0.05);
    }

    [Fact]
    public void CreateThumbnail_ShouldHandleWebPInput_Successfully()
    {
        // Arrange
        using var webpImageStream = TestImageHelper.CreateTestImage(1920, 1080, SKEncodedImageFormat.Webp);

        // Act
        var result = _service.CreateThumbnail(webpImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
    }

    [Theory]
    [InlineData(800, 600)]    // Landscape
    [InlineData(600, 800)]    // Portrait
    [InlineData(1000, 1000)]  // Square
    [InlineData(1920, 1080)]  // Full HD Landscape
    [InlineData(1080, 1920)]  // Full HD Portrait
    public void CreateThumbnail_ShouldHandleVariousDimensions_Successfully(int width, int height)
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(width, height);

        // Act
        var result = _service.CreateThumbnail(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify thumbnail is within limits
        using var thumbnailStream = new MemoryStream(result.Value);
        using var thumbnailBitmap = SKBitmap.Decode(thumbnailStream);
        Assert.True(thumbnailBitmap.Width <= ThumbnailService.ThumbnailWidth);
        Assert.True(thumbnailBitmap.Height <= ThumbnailService.ThumbnailHeight);
    }
}


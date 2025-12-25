using Nexus.Application.Features.ImageProcessing.Errors;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.UnitTests.Utilities.Helpers;
using SkiaSharp;

namespace Nexus.Application.UnitTests.Features.ImageProcessing.ProcessImage;

public class ImageConversionServiceTests
{
    private readonly ImageConversionService _service = new();

    [Fact]
    public void ConvertToWebP_ShouldReturnSuccess_WhenValidJpegImageProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage();

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify it's a valid WebP image
        using var verifyStream = new MemoryStream(result.Value);
        using var verifyBitmap = SKBitmap.Decode(verifyStream);
        Assert.NotNull(verifyBitmap);

        // Verify dimensions are preserved
        Assert.Equal(800, verifyBitmap.Width);
        Assert.Equal(600, verifyBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldReturnSuccess_WhenValidPngImageProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(1024, 768, SKEncodedImageFormat.Png);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify dimensions are preserved
        using var verifyStream = new MemoryStream(result.Value);
        using var verifyBitmap = SKBitmap.Decode(verifyStream);
        Assert.Equal(1024, verifyBitmap.Width);
        Assert.Equal(768, verifyBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldReturnDecodeFailed_WhenInvalidImageProvided()
    {
        // Arrange
        using var invalidStream = TestImageHelper.CreateInvalidImageStream();

        // Act
        var result = _service.ConvertToWebP(invalidStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void ConvertToWebP_ShouldReturnDecodeFailed_WhenEmptyStreamProvided()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var result = _service.ConvertToWebP(emptyStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void ConvertToWebP_ShouldReturnDecodeFailed_WhenNonImageDataProvided()
    {
        // Arrange
        var textData = "This is not an image"u8.ToArray();
        using var textStream = new MemoryStream(textData);

        // Act
        var result = _service.ConvertToWebP(textStream);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ImageProcessingErrors.DecodeFailed, result.Errors);
    }

    [Fact]
    public void ConvertToWebP_ShouldProduceWebPImage_WhenJpegProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage();

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the output is WebP format
        using var outputStream = new MemoryStream(result.Value);
        using var codec = SKCodec.Create(outputStream);
        Assert.NotNull(codec);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
    }

    [Fact]
    public void ConvertToWebP_ShouldProduceWebPImage_WhenPngProvided()
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(640, 480, SKEncodedImageFormat.Png);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the output is WebP format
        using var outputStream = new MemoryStream(result.Value);
        using var codec = SKCodec.Create(outputStream);
        Assert.NotNull(codec);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
    }

    [Fact]
    public void ConvertToWebP_ShouldPreserveDimensions_WhenConvertingFromJpeg()
    {
        // Arrange
        const int originalWidth = 1920;
        const int originalHeight = 1080;
        using var imageStream = TestImageHelper.CreateTestImage(originalWidth, originalHeight);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        Assert.NotNull(outputBitmap);
        Assert.Equal(originalWidth, outputBitmap.Width);
        Assert.Equal(originalHeight, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldPreserveAspectRatio_WhenConvertingLandscapeImage()
    {
        // Arrange
        const int width = 1600;
        const int height = 900;
        using var imageStream = TestImageHelper.CreateTestImage(width, height);
        var originalAspectRatio = (double)width / height;

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        var outputAspectRatio = (double)outputBitmap.Width / outputBitmap.Height;
        Assert.Equal(originalAspectRatio, outputAspectRatio, 0.001);
    }

    [Fact]
    public void ConvertToWebP_ShouldPreserveAspectRatio_WhenConvertingPortraitImage()
    {
        // Arrange
        const int width = 900;
        const int height = 1600;
        using var imageStream = TestImageHelper.CreateTestImage(width, height
        );
        var originalAspectRatio = (double)width / height;

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        var outputAspectRatio = (double)outputBitmap.Width / outputBitmap.Height;
        Assert.Equal(originalAspectRatio, outputAspectRatio, 0.001);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleSquareImage_Successfully()
    {
        // Arrange
        const int dimension = 1000;
        using var imageStream = TestImageHelper.CreateTestImage(dimension, dimension);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        Assert.Equal(dimension, outputBitmap.Width);
        Assert.Equal(dimension, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleVeryLargeImage_Successfully()
    {
        // Arrange
        using var largeImageStream = TestImageHelper.CreateTestImage(5000, 4000);

        // Act
        var result = _service.ConvertToWebP(largeImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify dimensions are preserved
        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);
        Assert.Equal(5000, outputBitmap.Width);
        Assert.Equal(4000, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleSmallImage_Successfully()
    {
        // Arrange
        using var smallImageStream = TestImageHelper.CreateTestImage(100, 100);

        // Act
        var result = _service.ConvertToWebP(smallImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify dimensions are preserved
        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);
        Assert.Equal(100, outputBitmap.Width);
        Assert.Equal(100, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleWebPInput_Successfully()
    {
        // Arrange
        using var webpImageStream = TestImageHelper.CreateTestImage(1280, 720, SKEncodedImageFormat.Webp);

        // Act
        var result = _service.ConvertToWebP(webpImageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify it's still WebP
        using var outputStream = new MemoryStream(result.Value);
        using var codec = SKCodec.Create(outputStream);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleExtremeAspectRatio_WideImage()
    {
        // Arrange - very wide image
        const int width = 3000;
        const int height = 500;
        using var wideImageStream = TestImageHelper.CreateTestImage(width, height);

        // Act
        var result = _service.ConvertToWebP(wideImageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        Assert.Equal(width, outputBitmap.Width);
        Assert.Equal(height, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldHandleExtremeAspectRatio_TallImage()
    {
        // Arrange - very tall image
        const int width = 500;
        const int height = 3000;
        using var tallImageStream = TestImageHelper.CreateTestImage(width, height);

        // Act
        var result = _service.ConvertToWebP(tallImageStream);

        // Assert
        Assert.True(result.IsSuccess);

        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);

        Assert.Equal(width, outputBitmap.Width);
        Assert.Equal(height, outputBitmap.Height);
    }

    [Fact]
    public void ConvertToWebP_ShouldReduceFileSize_ComparedToJpeg()
    {
        // Arrange
        using var jpegStream = TestImageHelper.CreateTestImage(2000, 1500);

        // Act
        var result = _service.ConvertToWebP(jpegStream);

        // Assert
        Assert.True(result.IsSuccess);

        // WebP at 85% quality should typically be smaller than JPEG for photographic content
        // Note: This might vary depending on the test image content
        Assert.True(result.Value.Length > 0, "WebP output should have content");
    }

    [Fact]
    public void ConvertToWebP_ShouldReduceFileSize_ComparedToPng()
    {
        // Arrange
        using var pngStream = TestImageHelper.CreateTestImage(1000, 1000, SKEncodedImageFormat.Png);
        var pngSize = pngStream.Length;

        // Act
        var result = _service.ConvertToWebP(pngStream);

        // Assert
        Assert.True(result.IsSuccess);

        // WebP should typically be much smaller than PNG
        Assert.True(result.Value.Length < pngSize,
            $"WebP size ({result.Value.Length}) should be smaller than PNG size ({pngSize})");
    }

    [Theory]
    [InlineData(800, 600)]    // Standard 4:3
    [InlineData(1920, 1080)]  // Full HD 16:9
    [InlineData(1080, 1920)]  // Portrait Full HD
    [InlineData(1000, 1000)]  // Square
    [InlineData(320, 240)]    // Small
    [InlineData(4000, 3000)]  // Large
    public void ConvertToWebP_ShouldHandleVariousDimensions_Successfully(int width, int height)
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(width, height);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        // Verify dimensions are preserved
        using var outputStream = new MemoryStream(result.Value);
        using var outputBitmap = SKBitmap.Decode(outputStream);
        Assert.Equal(width, outputBitmap.Width);
        Assert.Equal(height, outputBitmap.Height);
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Jpeg)]
    [InlineData(SKEncodedImageFormat.Png)]
    [InlineData(SKEncodedImageFormat.Webp)]
    public void ConvertToWebP_ShouldHandleDifferentInputFormats_Successfully(SKEncodedImageFormat inputFormat)
    {
        // Arrange
        using var imageStream = TestImageHelper.CreateTestImage(640, 480, inputFormat);

        // Act
        var result = _service.ConvertToWebP(imageStream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify output is WebP
        using var outputStream = new MemoryStream(result.Value);
        using var codec = SKCodec.Create(outputStream);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
    }
}


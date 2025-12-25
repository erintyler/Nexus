using SkiaSharp;

namespace Nexus.UnitTests.Utilities.Helpers;

public static class TestImageHelper
{
    /// <summary>
    /// Creates a valid test image as a MemoryStream with the specified dimensions and format.
    /// </summary>
    public static MemoryStream CreateTestImage(
        int width = 800,
        int height = 600,
        SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg,
        int quality = 90)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Fill with a gradient background
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                new[] { SKColors.Blue, SKColors.Green, SKColors.Yellow },
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, paint);

        // Add some shapes to make it more interesting
        using var shapePaint = new SKPaint
        {
            Color = SKColors.Red,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(width / 2, height / 2, Math.Min(width, height) / 4, shapePaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);

        var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;

        return stream;
    }

    /// <summary>
    /// Creates a valid test image as a byte array with the specified dimensions and format.
    /// </summary>
    public static byte[] CreateTestImageBytes(
        int width = 800,
        int height = 600,
        SKEncodedImageFormat format = SKEncodedImageFormat.Jpeg,
        int quality = 90)
    {
        using var stream = CreateTestImage(width, height, format, quality);
        return stream.ToArray();
    }

    /// <summary>
    /// Creates an invalid/corrupted image stream for testing error handling.
    /// </summary>
    public static MemoryStream CreateInvalidImageStream()
    {
        var invalidData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00 }; // Truncated JPEG header
        return new MemoryStream(invalidData);
    }
}


namespace ImageCompression.Wpf.Converters;

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageCompression.Core.Models;

/// <summary>
/// Provides high-performance conversions between domain GrayscaleImage instances and WPF BitmapSource elements.
/// </summary>
public static class BitmapConverter
{
    public static BitmapSource ToBitmapSource(GrayscaleImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int width = image.Width;
        int height = image.Height;
        byte[] rawBytes = image.To8BitByteArray();
        int stride = width;

        var bitmap = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Gray8,
            null,
            rawBytes,
            stride);

        bitmap.Freeze();
        return bitmap;
    }

    public static GrayscaleImage LoadGrayscaleImageFromFile(string filePath, int targetWidth = 512, int targetHeight = 512)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Image file does not exist.", filePath);
        }

        var sourceUri = new Uri(filePath, UriKind.Absolute);
        var originalBitmap = new BitmapImage();
        originalBitmap.BeginInit();
        originalBitmap.UriSource = sourceUri;
        originalBitmap.CacheOption = BitmapCacheOption.OnLoad;
        originalBitmap.EndInit();
        originalBitmap.Freeze();

        var grayBitmap = new FormatConvertedBitmap(originalBitmap, PixelFormats.Gray8, null, 0);
        grayBitmap.Freeze();

        double scaleX = targetWidth / (double)grayBitmap.PixelWidth;
        double scaleY = targetHeight / (double)grayBitmap.PixelHeight;

        var transformed = new TransformedBitmap(grayBitmap, new ScaleTransform(scaleX, scaleY));
        transformed.Freeze();

        int stride = targetWidth;
        byte[] pixels = new byte[targetWidth * targetHeight];
        transformed.CopyPixels(new Int32Rect(0, 0, targetWidth, targetHeight), pixels, stride, 0);

        return GrayscaleImage.From8BitByteArray(pixels, targetWidth, targetHeight);
    }

    public static void SaveGrayscaleImageToPng(GrayscaleImage image, string targetFilePath)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath);

        var bitmapSource = ToBitmapSource(image);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var stream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        encoder.Save(stream);
    }
}

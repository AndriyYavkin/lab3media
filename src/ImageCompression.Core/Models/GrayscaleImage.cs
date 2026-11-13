namespace ImageCompression.Core.Models;

/// <summary>
/// Represents a two-dimensional grayscale image stored with 64-bit precision.
/// </summary>
public sealed class GrayscaleImage
{
    private readonly double[,] _pixels;

    public int Width { get; }
    public int Height { get; }

    public double this[int x, int y]
    {
        get => _pixels[x, y];
        set => _pixels[x, y] = value;
    }

    public GrayscaleImage(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        Width = width;
        Height = height;
        _pixels = new double[width, height];
    }

    public GrayscaleImage(double[,] initialData)
    {
        ArgumentNullException.ThrowIfNull(initialData);

        Width = initialData.GetLength(0);
        Height = initialData.GetLength(1);

        if (Width <= 0 || Height <= 0)
        {
            throw new ArgumentException("Dimensions must be positive.", nameof(initialData));
        }

        _pixels = new double[Width, Height];
        Array.Copy(initialData, _pixels, initialData.Length);
    }

    public double[,] ToRawArray()
    {
        var copy = new double[Width, Height];
        Array.Copy(_pixels, copy, _pixels.Length);
        return copy;
    }

    public byte[] To8BitByteArray()
    {
        var bytes = new byte[Width * Height];
        int index = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                double value = _pixels[x, y];
                if (value < 0.0)
                {
                    value = 0.0;
                }
                else if (value > 255.0)
                {
                    value = 255.0;
                }

                bytes[index++] = (byte)Math.Round(value);
            }
        }

        return bytes;
    }

    public GrayscaleImage Clone()
    {
        return new GrayscaleImage(_pixels);
    }

    public static GrayscaleImage From8BitByteArray(byte[] bytes, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length != width * height)
        {
            throw new ArgumentException("Byte array length does not match specified dimensions.", nameof(bytes));
        }

        var image = new GrayscaleImage(width, height);
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                image[x, y] = bytes[index++];
            }
        }

        return image;
    }
}

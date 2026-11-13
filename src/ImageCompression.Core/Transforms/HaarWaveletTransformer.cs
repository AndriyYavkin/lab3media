namespace ImageCompression.Core.Transforms;

using ImageCompression.Core.Models;

/// <summary>
/// Implements the forward and inverse two-dimensional Discrete Wavelet Transform using the orthonormal Haar wavelet.
/// </summary>
public sealed class HaarWaveletTransformer : IDwtTransformer
{
    private static readonly double Sqrt2 = Math.Sqrt(2.0);

    public string Name => "Discrete Wavelet Transform (Haar Wavelet)";
    public string WaveletFamily => "Haar";

    public int DecompositionLevels { get; set; } = 4;

    public TransformResult Forward(GrayscaleImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int originalWidth = image.Width;
        int originalHeight = image.Height;

        int paddedWidth = NextPowerOfTwo(originalWidth);
        int paddedHeight = NextPowerOfTwo(originalHeight);

        var coefficients = new double[paddedWidth, paddedHeight];

        for (int y = 0; y < paddedHeight; y++)
        {
            int srcY = Math.Min(y, originalHeight - 1);
            for (int x = 0; x < paddedWidth; x++)
            {
                int srcX = Math.Min(x, originalWidth - 1);
                coefficients[x, y] = image[srcX, srcY];
            }
        }

        int maxPossibleLevels = Math.Min(GetPowerOfTwoExponent(paddedWidth), GetPowerOfTwoExponent(paddedHeight));
        int effectiveLevels = Math.Clamp(DecompositionLevels, 1, maxPossibleLevels);

        int currentW = paddedWidth;
        int currentH = paddedHeight;

        var tempRow = new double[paddedWidth];
        var tempCol = new double[paddedHeight];

        for (int level = 0; level < effectiveLevels; level++)
        {
            for (int y = 0; y < currentH; y++)
            {
                TransformRowForward(coefficients, y, currentW, tempRow);
            }

            for (int x = 0; x < currentW; x++)
            {
                TransformColumnForward(coefficients, x, currentH, tempCol);
            }

            currentW /= 2;
            currentH /= 2;
        }

        return new TransformResult(coefficients, Name);
    }

    public GrayscaleImage Inverse(TransformResult transformResult)
    {
        ArgumentNullException.ThrowIfNull(transformResult);

        int paddedWidth = transformResult.Width;
        int paddedHeight = transformResult.Height;

        var buffer = new double[paddedWidth, paddedHeight];
        Array.Copy(transformResult.Coefficients, buffer, transformResult.Coefficients.Length);

        int maxPossibleLevels = Math.Min(GetPowerOfTwoExponent(paddedWidth), GetPowerOfTwoExponent(paddedHeight));
        int effectiveLevels = Math.Clamp(DecompositionLevels, 1, maxPossibleLevels);

        var tempRow = new double[paddedWidth];
        var tempCol = new double[paddedHeight];

        for (int level = effectiveLevels - 1; level >= 0; level--)
        {
            int currentW = paddedWidth >> level;
            int currentH = paddedHeight >> level;

            for (int x = 0; x < currentW; x++)
            {
                TransformColumnInverse(buffer, x, currentH, tempCol);
            }

            for (int y = 0; y < currentH; y++)
            {
                TransformRowInverse(buffer, y, currentW, tempRow);
            }
        }

        var reconstructed = new GrayscaleImage(paddedWidth, paddedHeight);
        for (int y = 0; y < paddedHeight; y++)
        {
            for (int x = 0; x < paddedWidth; x++)
            {
                reconstructed[x, y] = Math.Clamp(buffer[x, y], 0.0, 255.0);
            }
        }

        return reconstructed;
    }

    private static void TransformRowForward(double[,] data, int y, int length, double[] temp)
    {
        int half = length / 2;
        for (int i = 0; i < half; i++)
        {
            double a = data[2 * i, y];
            double b = data[2 * i + 1, y];

            temp[i] = (a + b) / Sqrt2;
            temp[half + i] = (a - b) / Sqrt2;
        }

        for (int i = 0; i < length; i++)
        {
            data[i, y] = temp[i];
        }
    }

    private static void TransformColumnForward(double[,] data, int x, int length, double[] temp)
    {
        int half = length / 2;
        for (int i = 0; i < half; i++)
        {
            double a = data[x, 2 * i];
            double b = data[x, 2 * i + 1];

            temp[i] = (a + b) / Sqrt2;
            temp[half + i] = (a - b) / Sqrt2;
        }

        for (int i = 0; i < length; i++)
        {
            data[x, i] = temp[i];
        }
    }

    private static void TransformRowInverse(double[,] data, int y, int length, double[] temp)
    {
        int half = length / 2;
        for (int i = 0; i < half; i++)
        {
            double approx = data[i, y];
            double detail = data[half + i, y];

            temp[2 * i] = (approx + detail) / Sqrt2;
            temp[2 * i + 1] = (approx - detail) / Sqrt2;
        }

        for (int i = 0; i < length; i++)
        {
            data[i, y] = temp[i];
        }
    }

    private static void TransformColumnInverse(double[,] data, int x, int length, double[] temp)
    {
        int half = length / 2;
        for (int i = 0; i < half; i++)
        {
            double approx = data[x, i];
            double detail = data[x, half + i];

            temp[2 * i] = (approx + detail) / Sqrt2;
            temp[2 * i + 1] = (approx - detail) / Sqrt2;
        }

        for (int i = 0; i < length; i++)
        {
            data[x, i] = temp[i];
        }
    }

    private static int NextPowerOfTwo(int value)
    {
        int power = 1;
        while (power < value)
        {
            power <<= 1;
        }

        return power;
    }

    private static int GetPowerOfTwoExponent(int value)
    {
        int exponent = 0;
        while (value > 1)
        {
            value >>= 1;
            exponent++;
        }

        return exponent;
    }
}

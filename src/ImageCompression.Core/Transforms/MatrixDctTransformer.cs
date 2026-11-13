namespace ImageCompression.Core.Transforms;

using ImageCompression.Core.Models;

/// <summary>
/// Implements the 8x8 block-based Discrete Cosine Transform using explicit matrix multiplication.
/// </summary>
public sealed class MatrixDctTransformer : IDctTransformer
{
    private const int DefaultBlockSize = 8;
    private readonly double[,] _matrixT;
    private readonly double[,] _matrixTTranspose;

    public string Name => "Discrete Cosine Transform (8x8 Block DCT)";
    public int BlockSize => DefaultBlockSize;

    public MatrixDctTransformer()
    {
        _matrixT = GenerateDctMatrix(DefaultBlockSize);
        _matrixTTranspose = TransposeMatrix(_matrixT, DefaultBlockSize);
    }

    public double[,] GetTransformMatrix()
    {
        var copy = new double[DefaultBlockSize, DefaultBlockSize];
        Array.Copy(_matrixT, copy, _matrixT.Length);
        return copy;
    }

    public TransformResult Forward(GrayscaleImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int originalWidth = image.Width;
        int originalHeight = image.Height;

        int paddedWidth = ((originalWidth + DefaultBlockSize - 1) / DefaultBlockSize) * DefaultBlockSize;
        int paddedHeight = ((originalHeight + DefaultBlockSize - 1) / DefaultBlockSize) * DefaultBlockSize;

        var coefficients = new double[paddedWidth, paddedHeight];

        var block = new double[DefaultBlockSize, DefaultBlockSize];
        var temp = new double[DefaultBlockSize, DefaultBlockSize];
        var transformedBlock = new double[DefaultBlockSize, DefaultBlockSize];

        for (int by = 0; by < paddedHeight; by += DefaultBlockSize)
        {
            for (int bx = 0; bx < paddedWidth; bx += DefaultBlockSize)
            {
                for (int y = 0; y < DefaultBlockSize; y++)
                {
                    int srcY = Math.Min(by + y, originalHeight - 1);
                    for (int x = 0; x < DefaultBlockSize; x++)
                    {
                        int srcX = Math.Min(bx + x, originalWidth - 1);
                        block[y, x] = image[srcX, srcY] - 128.0;
                    }
                }

                MultiplyMatrices(block, _matrixTTranspose, temp, DefaultBlockSize);
                MultiplyMatrices(_matrixT, temp, transformedBlock, DefaultBlockSize);

                for (int y = 0; y < DefaultBlockSize; y++)
                {
                    for (int x = 0; x < DefaultBlockSize; x++)
                    {
                        coefficients[bx + x, by + y] = transformedBlock[y, x];
                    }
                }
            }
        }

        return new TransformResult(coefficients, Name);
    }

    public GrayscaleImage Inverse(TransformResult transformResult)
    {
        ArgumentNullException.ThrowIfNull(transformResult);

        int width = transformResult.Width;
        int height = transformResult.Height;

        var reconstructed = new GrayscaleImage(width, height);

        var block = new double[DefaultBlockSize, DefaultBlockSize];
        var temp = new double[DefaultBlockSize, DefaultBlockSize];
        var reconstructedBlock = new double[DefaultBlockSize, DefaultBlockSize];

        for (int by = 0; by < height; by += DefaultBlockSize)
        {
            for (int bx = 0; bx < width; bx += DefaultBlockSize)
            {
                for (int y = 0; y < DefaultBlockSize; y++)
                {
                    for (int x = 0; x < DefaultBlockSize; x++)
                    {
                        block[y, x] = transformResult.Coefficients[bx + x, by + y];
                    }
                }

                MultiplyMatrices(block, _matrixT, temp, DefaultBlockSize);
                MultiplyMatrices(_matrixTTranspose, temp, reconstructedBlock, DefaultBlockSize);

                for (int y = 0; y < DefaultBlockSize; y++)
                {
                    for (int x = 0; x < DefaultBlockSize; x++)
                    {
                        double pixelValue = reconstructedBlock[y, x] + 128.0;
                        reconstructed[bx + x, by + y] = Math.Clamp(pixelValue, 0.0, 255.0);
                    }
                }
            }
        }

        return reconstructed;
    }

    private static double[,] GenerateDctMatrix(int n)
    {
        var matrix = new double[n, n];
        double alpha0 = Math.Sqrt(1.0 / n);
        double alphaK = Math.Sqrt(2.0 / n);

        for (int i = 0; i < n; i++)
        {
            double alpha = (i == 0) ? alpha0 : alphaK;
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] = alpha * Math.Cos(((2.0 * j + 1.0) * i * Math.PI) / (2.0 * n));
            }
        }

        return matrix;
    }

    private static double[,] TransposeMatrix(double[,] source, int n)
    {
        var transposed = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                transposed[i, j] = source[j, i];
            }
        }

        return transposed;
    }

    private static void MultiplyMatrices(double[,] a, double[,] b, double[,] result, int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                {
                    sum += a[i, k] * b[k, j];
                }

                result[i, j] = sum;
            }
        }
    }
}

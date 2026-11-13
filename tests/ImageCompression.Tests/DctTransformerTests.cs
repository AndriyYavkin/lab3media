namespace ImageCompression.Tests;

using ImageCompression.Core.Models;
using ImageCompression.Core.Transforms;

public sealed class DctTransformerTests
{
    [Fact]
    public void DctMatrix_ShouldBeOrthogonal()
    {
        var transformer = new MatrixDctTransformer();
        double[,] matrix = transformer.GetTransformMatrix();
        int n = transformer.BlockSize;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double dot = 0.0;
                for (int k = 0; k < n; k++)
                {
                    dot += matrix[i, k] * matrix[j, k];
                }

                double expected = (i == j) ? 1.0 : 0.0;
                Assert.True(Math.Abs(dot - expected) < 1e-9, $"Orthogonality failed at [{i}, {j}]. Found {dot}, expected {expected}.");
            }
        }
    }

    [Fact]
    public void Roundtrip_WithoutQuantization_ShouldBeLossless()
    {
        var transformer = new MatrixDctTransformer();
        var original = new GrayscaleImage(16, 16);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                original[x, y] = (x * 13 + y * 7) % 256;
            }
        }

        var transformResult = transformer.Forward(original);
        var reconstructed = transformer.Inverse(transformResult);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                double diff = Math.Abs(original[x, y] - reconstructed[x, y]);
                Assert.True(diff < 1e-6, $"Reconstruction mismatch at ({x}, {y}): original {original[x, y]}, reconstructed {reconstructed[x, y]}");
            }
        }
    }
}

namespace ImageCompression.Tests;

using ImageCompression.Core.Models;
using ImageCompression.Core.Transforms;

public sealed class HaarWaveletTransformerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void Roundtrip_WithoutThresholding_ShouldBeLossless(int levels)
    {
        var transformer = new HaarWaveletTransformer
        {
            DecompositionLevels = levels
        };

        var original = new GrayscaleImage(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                original[x, y] = (x * 23 + y * 19 + 42) % 256;
            }
        }

        var transformResult = transformer.Forward(original);
        var reconstructed = transformer.Inverse(transformResult);

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                double diff = Math.Abs(original[x, y] - reconstructed[x, y]);
                Assert.True(diff < 1e-6, $"Reconstruction mismatch at ({x}, {y}): original {original[x, y]}, reconstructed {reconstructed[x, y]}");
            }
        }
    }
}

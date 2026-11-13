namespace ImageCompression.Tests;

using ImageCompression.Core.Compression;
using ImageCompression.Core.Metrics;
using ImageCompression.Core.Models;
using ImageCompression.Core.Services;
using ImageCompression.Core.Transforms;

public sealed class ImageProcessorServiceTests
{
    [Fact]
    public void Pipeline_WithDctAndTopCoefficients_ShouldProduceValidResult()
    {
        var service = new ImageProcessorService(new PsnrCalculator());
        var transformer = new MatrixDctTransformer();
        var strategy = new TopCoefficientsStrategy();
        var parameters = new CompressionParameters { RetentionPercentage = 10.0 };

        var image = new GrayscaleImage(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                image[x, y] = (x + y * 2) % 256;
            }
        }

        var result = service.Process(image, transformer, strategy, parameters);

        Assert.NotNull(result);
        Assert.NotNull(result.ReconstructedImage);
        Assert.NotNull(result.DifferenceMap);
        Assert.NotNull(result.CoefficientVisualizationMap);
        Assert.True(result.Metrics.Psnr > 0.0);
        Assert.True(result.Metrics.RetainedPercentage <= 10.5);
    }

    [Fact]
    public void Pipeline_WithDwtAndTopCoefficients_ShouldProduceValidResult()
    {
        var service = new ImageProcessorService(new PsnrCalculator());
        var transformer = new HaarWaveletTransformer { DecompositionLevels = 3 };
        var strategy = new TopCoefficientsStrategy();
        var parameters = new CompressionParameters { RetentionPercentage = 5.0 };

        var image = new GrayscaleImage(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                image[x, y] = (x * 3 + y * 5) % 256;
            }
        }

        var result = service.Process(image, transformer, strategy, parameters);

        Assert.NotNull(result);
        Assert.NotNull(result.ReconstructedImage);
        Assert.NotNull(result.DifferenceMap);
        Assert.NotNull(result.CoefficientVisualizationMap);
        Assert.True(result.Metrics.Psnr > 0.0);
        Assert.True(result.Metrics.RetainedPercentage <= 5.5);
    }
}

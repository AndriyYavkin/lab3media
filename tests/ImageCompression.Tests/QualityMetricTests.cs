namespace ImageCompression.Tests;

using ImageCompression.Core.Metrics;
using ImageCompression.Core.Models;

public sealed class QualityMetricTests
{
    [Fact]
    public void PsnrCalculator_IdenticalImages_ShouldReturnPositiveInfinity()
    {
        var calculator = new PsnrCalculator();
        var img1 = new GrayscaleImage(8, 8);
        var img2 = new GrayscaleImage(8, 8);
        var coefficients = new double[8, 8];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                img1[x, y] = 120.0;
                img2[x, y] = 120.0;
                coefficients[x, y] = 1.0;
            }
        }

        var metrics = calculator.Calculate(img1, img2, coefficients, 5.0);

        Assert.Equal(0.0, metrics.Mse);
        Assert.True(double.IsPositiveInfinity(metrics.Psnr));
        Assert.Equal(64, metrics.RetainedCoefficients);
        Assert.Equal(100.0, metrics.RetainedPercentage);
    }

    [Fact]
    public void PsnrCalculator_WithKnownError_ShouldCalculateAccuratePsnr()
    {
        var calculator = new PsnrCalculator();
        var img1 = new GrayscaleImage(2, 2);
        var img2 = new GrayscaleImage(2, 2);
        var coefficients = new double[2, 2];

        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                img1[x, y] = 100.0;
                img2[x, y] = 105.0;
            }
        }

        var metrics = calculator.Calculate(img1, img2, coefficients, 1.0);

        double expectedMse = 25.0;
        double expectedPsnr = 10.0 * Math.Log10((255.0 * 255.0) / expectedMse);

        Assert.True(Math.Abs(metrics.Mse - expectedMse) < 1e-6);
        Assert.True(Math.Abs(metrics.Psnr - expectedPsnr) < 1e-4);
    }
}

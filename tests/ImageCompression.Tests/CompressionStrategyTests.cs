namespace ImageCompression.Tests;

using ImageCompression.Core.Compression;

public sealed class CompressionStrategyTests
{
    [Fact]
    public void TopCoefficientsStrategy_ShouldRetainExactPercentage()
    {
        var strategy = new TopCoefficientsStrategy();
        var coefficients = new double[10, 10];
        double val = 1.0;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                coefficients[x, y] = val++;
            }
        }

        var parameters = new CompressionParameters
        {
            RetentionPercentage = 20.0
        };

        strategy.ProcessCoefficients(coefficients, parameters);

        int nonZeroCount = 0;
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (Math.Abs(coefficients[x, y]) > 1e-9)
                {
                    nonZeroCount++;
                }
            }
        }

        Assert.Equal(20, nonZeroCount);
    }

    [Fact]
    public void ThresholdCompressionStrategy_ShouldZeroOutSmallValues()
    {
        var strategy = new ThresholdCompressionStrategy();
        var coefficients = new double[,]
        {
            { 5.0, 15.0 },
            { 2.0, 25.0 }
        };

        var parameters = new CompressionParameters
        {
            AbsoluteThreshold = 10.0
        };

        strategy.ProcessCoefficients(coefficients, parameters);

        Assert.Equal(0.0, coefficients[0, 0]);
        Assert.Equal(15.0, coefficients[0, 1]);
        Assert.Equal(0.0, coefficients[1, 0]);
        Assert.Equal(25.0, coefficients[1, 1]);
    }

    [Fact]
    public void QuantizationCompressionStrategy_ShouldDiscretizeValues()
    {
        var strategy = new QuantizationCompressionStrategy();
        var coefficients = new double[,]
        {
            { 7.0, 14.0 },
            { 21.0, 3.0 }
        };

        var parameters = new CompressionParameters
        {
            QuantizationStep = 10.0
        };

        strategy.ProcessCoefficients(coefficients, parameters);

        Assert.Equal(10.0, coefficients[0, 0]);
        Assert.Equal(10.0, coefficients[0, 1]);
        Assert.Equal(20.0, coefficients[1, 0]);
        Assert.Equal(0.0, coefficients[1, 1]);
    }
}

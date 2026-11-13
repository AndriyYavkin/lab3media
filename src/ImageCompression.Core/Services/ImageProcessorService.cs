namespace ImageCompression.Core.Services;

using System.Diagnostics;
using ImageCompression.Core.Compression;
using ImageCompression.Core.Metrics;
using ImageCompression.Core.Models;
using ImageCompression.Core.Transforms;

/// <summary>
/// Default implementation orchestrating the end-to-end compression pipeline and quality evaluation.
/// </summary>
public sealed class ImageProcessorService : IImageProcessorService
{
    private readonly IQualityMetricCalculator _metricCalculator;

    public ImageProcessorService(IQualityMetricCalculator metricCalculator)
    {
        _metricCalculator = metricCalculator ?? throw new ArgumentNullException(nameof(metricCalculator));
    }

    public CompressionEvaluationResult Process(
        GrayscaleImage sourceImage,
        IImageTransformer transformer,
        ICompressionStrategy strategy,
        CompressionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(sourceImage);
        ArgumentNullException.ThrowIfNull(transformer);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(parameters);

        var stopwatch = Stopwatch.StartNew();

        var transformResult = transformer.Forward(sourceImage);

        strategy.ProcessCoefficients(transformResult.Coefficients, parameters);

        var reconstructed = transformer.Inverse(transformResult);

        stopwatch.Stop();
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

        var metrics = _metricCalculator.Calculate(
            sourceImage,
            reconstructed,
            transformResult.Coefficients,
            elapsedMs);

        var differenceMap = CreateDifferenceMap(sourceImage, reconstructed);
        var coefficientMap = CreateCoefficientMap(transformResult.Coefficients);

        return new CompressionEvaluationResult(
            reconstructed,
            transformResult,
            metrics,
            differenceMap,
            coefficientMap);
    }

    private static GrayscaleImage CreateDifferenceMap(GrayscaleImage original, GrayscaleImage reconstructed)
    {
        int width = Math.Min(original.Width, reconstructed.Width);
        int height = Math.Min(original.Height, reconstructed.Height);
        var diffImage = new GrayscaleImage(width, height);

        const double amplificationFactor = 5.0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double delta = Math.Abs(original[x, y] - reconstructed[x, y]);
                diffImage[x, y] = Math.Clamp(delta * amplificationFactor, 0.0, 255.0);
            }
        }

        return diffImage;
    }

    private static GrayscaleImage CreateCoefficientMap(double[,] coefficients)
    {
        int width = coefficients.GetLength(0);
        int height = coefficients.GetLength(1);
        var map = new GrayscaleImage(width, height);

        double maxLog = 0.0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double mag = Math.Abs(coefficients[x, y]);
                double logVal = Math.Log(1.0 + mag);
                if (logVal > maxLog)
                {
                    maxLog = logVal;
                }
            }
        }

        double scale = maxLog > 1e-9 ? 255.0 / maxLog : 0.0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double logVal = Math.Log(1.0 + Math.Abs(coefficients[x, y]));
                map[x, y] = Math.Clamp(logVal * scale, 0.0, 255.0);
            }
        }

        return map;
    }
}

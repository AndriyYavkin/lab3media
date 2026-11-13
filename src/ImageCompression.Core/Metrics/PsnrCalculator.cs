namespace ImageCompression.Core.Metrics;

using ImageCompression.Core.Models;

/// <summary>
/// Computes Mean Squared Error (MSE), Peak Signal-to-Noise Ratio (PSNR), and coefficient sparsity.
/// </summary>
public sealed class PsnrCalculator : IQualityMetricCalculator
{
    private const double MaxIntensity = 255.0;
    private const double ZeroTolerance = 1e-9;

    public QualityMetrics Calculate(
        GrayscaleImage original,
        GrayscaleImage reconstructed,
        double[,] coefficients,
        double elapsedMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(reconstructed);
        ArgumentNullException.ThrowIfNull(coefficients);

        int commonWidth = Math.Min(original.Width, reconstructed.Width);
        int commonHeight = Math.Min(original.Height, reconstructed.Height);
        int pixelCount = commonWidth * commonHeight;

        double sumSquaredError = 0.0;
        double sumAbsoluteError = 0.0;

        for (int y = 0; y < commonHeight; y++)
        {
            for (int x = 0; x < commonWidth; x++)
            {
                double diff = original[x, y] - reconstructed[x, y];
                sumSquaredError += diff * diff;
                sumAbsoluteError += Math.Abs(diff);
            }
        }

        double mse = pixelCount > 0 ? sumSquaredError / pixelCount : 0.0;
        double mae = pixelCount > 0 ? sumAbsoluteError / pixelCount : 0.0;

        double psnr = mse <= 1e-12
            ? double.PositiveInfinity
            : 10.0 * Math.Log10((MaxIntensity * MaxIntensity) / mse);

        int coeffWidth = coefficients.GetLength(0);
        int coeffHeight = coefficients.GetLength(1);
        int totalCoefficients = coeffWidth * coeffHeight;
        int retainedCount = 0;

        for (int y = 0; y < coeffHeight; y++)
        {
            for (int x = 0; x < coeffWidth; x++)
            {
                if (Math.Abs(coefficients[x, y]) > ZeroTolerance)
                {
                    retainedCount++;
                }
            }
        }

        double retainedPercentage = totalCoefficients > 0
            ? (retainedCount / (double)totalCoefficients) * 100.0
            : 0.0;

        double compressionRatio = retainedCount > 0
            ? totalCoefficients / (double)retainedCount
            : totalCoefficients;

        return new QualityMetrics(
            Mse: mse,
            Psnr: psnr,
            Mae: mae,
            TotalCoefficients: totalCoefficients,
            RetainedCoefficients: retainedCount,
            RetainedPercentage: retainedPercentage,
            CompressionRatio: compressionRatio,
            ElapsedMilliseconds: elapsedMilliseconds);
    }
}

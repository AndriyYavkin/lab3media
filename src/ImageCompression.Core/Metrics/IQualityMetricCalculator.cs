namespace ImageCompression.Core.Metrics;

using ImageCompression.Core.Models;

/// <summary>
/// Service contract for computing numerical error and compression efficiency metrics.
/// </summary>
public interface IQualityMetricCalculator
{
    QualityMetrics Calculate(
        GrayscaleImage original,
        GrayscaleImage reconstructed,
        double[,] coefficients,
        double elapsedMilliseconds);
}

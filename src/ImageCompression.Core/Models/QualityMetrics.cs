namespace ImageCompression.Core.Models;

/// <summary>
/// Contains experimental metrics evaluating reconstruction fidelity and compression performance.
/// </summary>
public sealed record QualityMetrics(
    double Mse,
    double Psnr,
    double Mae,
    int TotalCoefficients,
    int RetainedCoefficients,
    double RetainedPercentage,
    double CompressionRatio,
    double ElapsedMilliseconds)
{
    public string FormattedPsnr => double.IsPositiveInfinity(Psnr)
        ? "Infinity (Lossless)"
        : $"{Psnr:F2} dB";

    public string FormattedMse => $"{Mse:F4}";

    public string FormattedMae => $"{Mae:F4}";

    public string FormattedRetainedPercentage => $"{RetainedPercentage:F2}%";

    public string FormattedCompressionRatio => $"{CompressionRatio:F2}:1";

    public string FormattedExecutionTime => $"{ElapsedMilliseconds:F1} ms";
}

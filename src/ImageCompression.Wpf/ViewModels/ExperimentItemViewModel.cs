namespace ImageCompression.Wpf.ViewModels;

/// <summary>
/// Represents a row in the multi-level comparative experiment evaluation matrix.
/// </summary>
public sealed class ExperimentItemViewModel
{
    public string CompressionLevel { get; init; } = string.Empty;
    public double RetainedPercentage { get; init; }
    public int RetainedCoefficients { get; init; }
    public double DctPsnr { get; init; }
    public double DwtPsnr { get; init; }
    public double PsnrDifference => DwtPsnr - DctPsnr;
    public double DctMse { get; init; }
    public double DwtMse { get; init; }
    public double DctTimeMs { get; init; }
    public double DwtTimeMs { get; init; }

    public string FormattedDctPsnr => double.IsPositiveInfinity(DctPsnr) ? "Infinity" : $"{DctPsnr:F2} dB";
    public string FormattedDwtPsnr => double.IsPositiveInfinity(DwtPsnr) ? "Infinity" : $"{DwtPsnr:F2} dB";
    public string FormattedDifference => $"{PsnrDifference:+0.00;-0.00;0.00} dB";

    public string AdvantageSummary
    {
        get
        {
            if (Math.Abs(PsnrDifference) < 0.05)
            {
                return "Equivalent";
            }

            return PsnrDifference > 0 ? "DWT Advantage" : "DCT Advantage";
        }
    }
}

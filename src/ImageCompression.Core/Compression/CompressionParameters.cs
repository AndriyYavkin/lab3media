namespace ImageCompression.Core.Compression;

/// <summary>
/// Parameters governing coefficient retention, thresholding, and quantization.
/// </summary>
public sealed class CompressionParameters
{
    public double RetentionPercentage { get; set; } = 10.0;

    public double AbsoluteThreshold { get; set; } = 20.0;

    public double QuantizationStep { get; set; } = 16.0;

    public int FrequencyCutoffLimit { get; set; } = 4;
}

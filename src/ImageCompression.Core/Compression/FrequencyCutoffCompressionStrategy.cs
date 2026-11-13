namespace ImageCompression.Core.Compression;

/// <summary>
/// Truncates high-frequency spatial components within transform sub-blocks or high-frequency bands.
/// </summary>
public sealed class FrequencyCutoffCompressionStrategy : ICompressionStrategy
{
    public string Name => "High-Frequency Cutoff";
    public string Description => "Preserves low-frequency basis components within blocks and zeroes out fine high-frequency details.";

    public void ProcessCoefficients(double[,] coefficients, CompressionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ArgumentNullException.ThrowIfNull(parameters);

        int limit = Math.Clamp(parameters.FrequencyCutoffLimit, 0, 14);
        int width = coefficients.GetLength(0);
        int height = coefficients.GetLength(1);

        for (int blockY = 0; blockY < height; blockY += 8)
        {
            for (int blockX = 0; blockX < width; blockX += 8)
            {
                int maxU = Math.Min(8, width - blockX);
                int maxV = Math.Min(8, height - blockY);

                for (int u = 0; u < maxU; u++)
                {
                    for (int v = 0; v < maxV; v++)
                    {
                        if (u + v > limit)
                        {
                            coefficients[blockX + u, blockY + v] = 0.0;
                        }
                    }
                }
            }
        }
    }
}

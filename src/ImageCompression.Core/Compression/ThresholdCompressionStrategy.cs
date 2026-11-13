namespace ImageCompression.Core.Compression;

/// <summary>
/// Eliminates coefficients whose absolute value is strictly below a fixed threshold.
/// </summary>
public sealed class ThresholdCompressionStrategy : ICompressionStrategy
{
    public string Name => "Absolute Thresholding";
    public string Description => "Zeroes all transform coefficients with magnitude below a specified threshold.";

    public void ProcessCoefficients(double[,] coefficients, CompressionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ArgumentNullException.ThrowIfNull(parameters);

        double threshold = Math.Max(0.0, parameters.AbsoluteThreshold);
        if (threshold <= 0.0)
        {
            return;
        }

        int width = coefficients.GetLength(0);
        int height = coefficients.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Math.Abs(coefficients[x, y]) < threshold)
                {
                    coefficients[x, y] = 0.0;
                }
            }
        }
    }
}

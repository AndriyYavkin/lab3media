namespace ImageCompression.Core.Compression;

/// <summary>
/// Retains only the specified top percentage of transform coefficients with the largest absolute magnitude.
/// </summary>
public sealed class TopCoefficientsStrategy : ICompressionStrategy
{
    public string Name => "Top Coefficients Retention";
    public string Description => "Retains the highest-energy coefficients by absolute magnitude, zeroing all others.";

    public void ProcessCoefficients(double[,] coefficients, CompressionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ArgumentNullException.ThrowIfNull(parameters);

        int width = coefficients.GetLength(0);
        int height = coefficients.GetLength(1);
        int total = width * height;

        if (total == 0)
        {
            return;
        }

        double percentage = Math.Clamp(parameters.RetentionPercentage, 0.0, 100.0);
        int countToKeep = (int)Math.Round(total * (percentage / 100.0));

        if (countToKeep <= 0)
        {
            Array.Clear(coefficients, 0, total);
            return;
        }

        if (countToKeep >= total)
        {
            return;
        }

        var magnitudes = new double[total];
        int idx = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                magnitudes[idx++] = Math.Abs(coefficients[x, y]);
            }
        }

        Array.Sort(magnitudes);
        double threshold = magnitudes[total - countToKeep];

        int retained = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double mag = Math.Abs(coefficients[x, y]);
                if (mag > threshold)
                {
                    retained++;
                }
            }
        }

        int remainingAllowed = countToKeep - retained;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double mag = Math.Abs(coefficients[x, y]);
                if (mag < threshold)
                {
                    coefficients[x, y] = 0.0;
                }
                else if (Math.Abs(mag - threshold) < 1e-12)
                {
                    if (remainingAllowed > 0)
                    {
                        remainingAllowed--;
                    }
                    else
                    {
                        coefficients[x, y] = 0.0;
                    }
                }
            }
        }
    }
}

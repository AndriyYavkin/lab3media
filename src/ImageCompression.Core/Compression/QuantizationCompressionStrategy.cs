namespace ImageCompression.Core.Compression;

/// <summary>
/// Discretizes transform coefficients using a uniform scalar quantization step.
/// </summary>
public sealed class QuantizationCompressionStrategy : ICompressionStrategy
{
    public string Name => "Uniform Quantization";
    public string Description => "Divides coefficients by a step size, rounds to nearest integer, and reconstructs quantized values.";

    public void ProcessCoefficients(double[,] coefficients, CompressionParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ArgumentNullException.ThrowIfNull(parameters);

        double step = Math.Max(0.001, parameters.QuantizationStep);
        int width = coefficients.GetLength(0);
        int height = coefficients.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double quantized = Math.Round(coefficients[x, y] / step);
                coefficients[x, y] = quantized * step;
            }
        }
    }
}

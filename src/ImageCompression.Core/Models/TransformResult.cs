namespace ImageCompression.Core.Models;

/// <summary>
/// Encapsulates the coefficient representation produced by a forward transform.
/// </summary>
public sealed class TransformResult
{
    public double[,] Coefficients { get; }
    public int Width { get; }
    public int Height { get; }
    public string TransformerName { get; }

    public TransformResult(double[,] coefficients, string transformerName)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ArgumentException.ThrowIfNullOrWhiteSpace(transformerName);

        Width = coefficients.GetLength(0);
        Height = coefficients.GetLength(1);
        TransformerName = transformerName;

        Coefficients = new double[Width, Height];
        Array.Copy(coefficients, Coefficients, coefficients.Length);
    }

    public int CountNonZeroCoefficients(double tolerance = 1e-9)
    {
        int nonZeroCount = 0;
        int w = Width;
        int h = Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (Math.Abs(Coefficients[x, y]) > tolerance)
                {
                    nonZeroCount++;
                }
            }
        }

        return nonZeroCount;
    }

    public TransformResult Clone()
    {
        return new TransformResult(Coefficients, TransformerName);
    }
}

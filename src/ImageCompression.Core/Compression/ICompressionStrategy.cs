namespace ImageCompression.Core.Compression;

/// <summary>
/// Abstraction defining a strategy for pruning or quantizing transform coefficients.
/// </summary>
public interface ICompressionStrategy
{
    string Name { get; }
    string Description { get; }

    void ProcessCoefficients(double[,] coefficients, CompressionParameters parameters);
}

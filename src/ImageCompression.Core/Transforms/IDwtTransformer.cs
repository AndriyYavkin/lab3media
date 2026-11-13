namespace ImageCompression.Core.Transforms;

/// <summary>
/// Specialized contract for multi-scale Discrete Wavelet Transform operations.
/// </summary>
public interface IDwtTransformer : IImageTransformer
{
    string WaveletFamily { get; }

    int DecompositionLevels { get; set; }
}

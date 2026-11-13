namespace ImageCompression.Core.Transforms;

/// <summary>
/// Specialized contract for block-based Discrete Cosine Transform operations.
/// </summary>
public interface IDctTransformer : IImageTransformer
{
    int BlockSize { get; }

    double[,] GetTransformMatrix();
}

namespace ImageCompression.Core.Transforms;

using ImageCompression.Core.Models;

/// <summary>
/// Generic interface for reversible orthogonal two-dimensional image transformations.
/// </summary>
public interface IImageTransformer
{
    string Name { get; }

    TransformResult Forward(GrayscaleImage image);

    GrayscaleImage Inverse(TransformResult transformResult);
}

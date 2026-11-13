namespace ImageCompression.Core.Services;

using ImageCompression.Core.Compression;
using ImageCompression.Core.Models;
using ImageCompression.Core.Transforms;

/// <summary>
/// Result bundle containing the reconstructed image, transformed coefficients, error map, and quality metrics.
/// </summary>
public sealed record CompressionEvaluationResult(
    GrayscaleImage ReconstructedImage,
    TransformResult TransformResult,
    QualityMetrics Metrics,
    GrayscaleImage DifferenceMap,
    GrayscaleImage CoefficientVisualizationMap);

/// <summary>
/// Service coordinating forward transformation, compression strategy application, inverse reconstruction, and metric evaluation.
/// </summary>
public interface IImageProcessorService
{
    CompressionEvaluationResult Process(
        GrayscaleImage sourceImage,
        IImageTransformer transformer,
        ICompressionStrategy strategy,
        CompressionParameters parameters);
}

namespace ImageCompression.Core.Generators;

using ImageCompression.Core.Models;

/// <summary>
/// Factory interface for generating synthetic reference grayscale benchmark images.
/// </summary>
public interface ISampleImageGenerator
{
    string Title { get; }
    string Description { get; }

    GrayscaleImage Generate(int width = 512, int height = 512);
}

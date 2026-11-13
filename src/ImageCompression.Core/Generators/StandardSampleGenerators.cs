namespace ImageCompression.Core.Generators;

using ImageCompression.Core.Models;

/// <summary>
/// Benchmark generator creating geometric shapes with varying edge orientations and soft shading.
/// </summary>
public sealed class GeometricShapesGenerator : ISampleImageGenerator
{
    public string Title => "Geometric Shapes & Edges";
    public string Description => "Sharp edges, diagonal boundaries, and shaded circles designed to highlight blocking and ringing.";

    public GrayscaleImage Generate(int width = 512, int height = 512)
    {
        var image = new GrayscaleImage(width, height);
        double centerX = width / 2.0;
        double centerY = height / 2.0;
        double maxRadius = Math.Min(width, height) * 0.35;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double baseIntensity = 30.0 + (x / (double)width) * 40.0 + (y / (double)height) * 40.0;

                double dx = x - centerX;
                double dy = y - centerY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist <= maxRadius)
                {
                    double normalDist = dist / maxRadius;
                    double sphereShade = Math.Sqrt(Math.Max(0.0, 1.0 - normalDist * normalDist));
                    baseIntensity = 80.0 + 160.0 * sphereShade;
                }
                else if (x > width * 0.65 && y > height * 0.65)
                {
                    baseIntensity = ((x / 16 + y / 16) % 2 == 0) ? 220.0 : 40.0;
                }
                else if (Math.Abs(x - y) < 8 || Math.Abs(x + y - width) < 8)
                {
                    baseIntensity = 240.0;
                }
                else if (x < width * 0.25 && y < height * 0.25)
                {
                    baseIntensity = 190.0;
                }

                image[x, y] = Math.Clamp(baseIntensity, 0.0, 255.0);
            }
        }

        return image;
    }
}

/// <summary>
/// Benchmark generator producing concentric sinusoidal chirp rings to evaluate spatial frequency response.
/// </summary>
public sealed class RadialZonePlateGenerator : ISampleImageGenerator
{
    public string Title => "Radial Zone Plate";
    public string Description => "Concentric sinusoidal frequency chirp sweeping from zero to high frequency towards perimeter.";

    public GrayscaleImage Generate(int width = 512, int height = 512)
    {
        var image = new GrayscaleImage(width, height);
        double centerX = width / 2.0;
        double centerY = height / 2.0;
        double maxDistanceSq = centerX * centerX + centerY * centerY;
        double maxCycles = 48.0 * Math.PI;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double dx = x - centerX;
                double dy = y - centerY;
                double rSquared = (dx * dx + dy * dy) / maxDistanceSq;

                double value = 127.5 * (1.0 + Math.Cos(maxCycles * rSquared));
                image[x, y] = Math.Clamp(value, 0.0, 255.0);
            }
        }

        return image;
    }
}

/// <summary>
/// Benchmark generator creating a multi-scale checkerboard grid aligned and unaligned with 8x8 DCT blocks.
/// </summary>
public sealed class CheckerboardGenerator : ISampleImageGenerator
{
    public string Title => "Checkerboard & Grid Matrix";
    public string Description => "Multi-scale checkerboard pattern testing 8x8 block boundary alignment vs wavelet subbands.";

    public GrayscaleImage Generate(int width = 512, int height = 512)
    {
        var image = new GrayscaleImage(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int blockSize;
                if (x < width / 2 && y < height / 2)
                {
                    blockSize = 8;
                }
                else if (x >= width / 2 && y < height / 2)
                {
                    blockSize = 16;
                }
                else if (x < width / 2 && y >= height / 2)
                {
                    blockSize = 32;
                }
                else
                {
                    blockSize = 64;
                }

                bool isWhite = ((x / blockSize) + (y / blockSize)) % 2 == 0;
                double intensity = isWhite ? 230.0 : 25.0;

                if (x % blockSize == 0 || y % blockSize == 0)
                {
                    intensity = 128.0;
                }

                image[x, y] = intensity;
            }
        }

        return image;
    }
}

/// <summary>
/// Benchmark generator creating smooth 2D continuous harmonic gradients.
/// </summary>
public sealed class HarmonicGradientGenerator : ISampleImageGenerator
{
    public string Title => "Harmonic Gradient & Texture";
    public string Description => "Continuous sinusoidal waves for inspecting quantization contouring and low-frequency artifacts.";

    public GrayscaleImage Generate(int width = 512, int height = 512)
    {
        var image = new GrayscaleImage(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double u = (x / (double)width) * 4.0 * Math.PI;
                double v = (y / (double)height) * 4.0 * Math.PI;

                double wave1 = Math.Sin(u);
                double wave2 = Math.Cos(v);
                double wave3 = Math.Sin(u + v);

                double combined = (wave1 + wave2 + wave3) / 3.0;
                double intensity = 127.5 + 115.0 * combined;

                image[x, y] = Math.Clamp(intensity, 0.0, 255.0);
            }
        }

        return image;
    }
}

/// <summary>
/// Benchmark generator creating a synthetic natural scene with skyline, trees, and detailed textures.
/// </summary>
public sealed class SyntheticLandscapeGenerator : ISampleImageGenerator
{
    public string Title => "Synthetic Landscape & Textures";
    public string Description => "Natural scene featuring sky gradient, sharp horizons, fine foliage noise, and architectural elements.";

    public GrayscaleImage Generate(int width = 512, int height = 512)
    {
        var image = new GrayscaleImage(width, height);
        var random = new Random(42);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double intensity;

                if (y < height * 0.45)
                {
                    intensity = 240.0 - (y / (height * 0.45)) * 110.0;
                    double sunDx = x - width * 0.75;
                    double sunDy = y - height * 0.2;
                    double sunDist = Math.Sqrt(sunDx * sunDx + sunDy * sunDy);
                    if (sunDist < 35.0)
                    {
                        intensity = 255.0;
                    }
                }
                else if (y < height * 0.55)
                {
                    intensity = 70.0 + 30.0 * Math.Sin(x * 0.05);
                    if ((x / 20) % 2 == 0 && y > height * 0.48)
                    {
                        intensity = 45.0;
                    }
                }
                else
                {
                    double baseGround = 130.0 - ((y - height * 0.55) / (height * 0.45)) * 50.0;
                    double pseudoNoise = (random.NextDouble() - 0.5) * 35.0;
                    intensity = baseGround + pseudoNoise;
                }

                image[x, y] = Math.Clamp(intensity, 0.0, 255.0);
            }
        }

        return image;
    }
}

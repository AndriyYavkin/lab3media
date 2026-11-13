namespace ImageCompression.Tests;

using ImageCompression.Core.Generators;

public sealed class BenchmarkGeneratorTests
{
    [Fact]
    public void AllGenerators_ShouldProduceValid512x512ImagesWithinBounds()
    {
        ISampleImageGenerator[] generators =
        {
            new GeometricShapesGenerator(),
            new RadialZonePlateGenerator(),
            new CheckerboardGenerator(),
            new HarmonicGradientGenerator(),
            new SyntheticLandscapeGenerator()
        };

        foreach (var generator in generators)
        {
            var image = generator.Generate(512, 512);
            Assert.Equal(512, image.Width);
            Assert.Equal(512, image.Height);

            for (int y = 0; y < 512; y += 32)
            {
                for (int x = 0; x < 512; x += 32)
                {
                    double val = image[x, y];
                    Assert.True(val >= 0.0 && val <= 255.0, $"Generator {generator.Title} produced out of bound pixel value {val} at ({x}, {y})");
                }
            }
        }
    }
}

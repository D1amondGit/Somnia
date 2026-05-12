using Somnia.Game.Services.World;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class PerlinNoise2DTests
{
    [Test]
    public void SameSeed_SameSample()
    {
        var a = new PerlinNoise2D(999);
        var b = new PerlinNoise2D(999);
        Assert.That(a.Noise(12.34f, 56.78f), Is.EqualTo(b.Noise(12.34f, 56.78f)));
    }

    [Test]
    public void DifferentSeed_DifferentSample()
    {
        var a = new PerlinNoise2D(1);
        var b = new PerlinNoise2D(2);
        Assert.That(a.Noise(0.5f, 0.5f), Is.Not.EqualTo(b.Noise(0.5f, 0.5f)));
    }
}

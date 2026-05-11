using System.Linq;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class HexagonModelTests
{
    [Test]
    public void GetTopVertices_ReturnsSixPoints()
    {
        var h = new HexagonModel(Vector2.Zero, 50f, 40f, 0.7f, 0.04f);
        Assert.That(h.GetTopVertices().Count, Is.EqualTo(6));
    }

    [Test]
    public void RotationRadians_RotatesVerticesAroundCenter()
    {
        var center = new Vector2(100, 100);
        var notRotated = new HexagonModel(center, 50f, 0f, squash: 1f, tilt: 0f, rotationRadians: 0f);
        var rotated = new HexagonModel(center, 50f, 0f, squash: 1f, tilt: 0f, rotationRadians: MathHelper.PiOver2);

        var a = notRotated.GetTopVertices()[0];
        var b = rotated.GetTopVertices()[0];

        // Точка (radius, 0) после поворота на 90° должна стать (0, radius).
        Assert.That(a, Is.EqualTo(new Vector2(150, 100)));
        Assert.That(b.X, Is.EqualTo(100).Within(0.01f));
        Assert.That(b.Y, Is.EqualTo(150).Within(0.01f));
    }

    [Test]
    public void RotationRadians_PreservesDistanceToCenter()
    {
        var center = new Vector2(50, 50);
        var hex = new HexagonModel(center, 30f, 0f, squash: 1f, tilt: 0f, rotationRadians: 0.7f);
        foreach (var v in hex.GetTopVertices())
            Assert.That(Vector2.Distance(v, center), Is.EqualTo(30f).Within(0.01f));
    }
}

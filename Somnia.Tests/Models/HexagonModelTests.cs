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
}

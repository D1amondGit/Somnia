using Somnia.Game.Models;

namespace Somnia.Tests.Models;

[TestFixture]
public sealed class ResourceDropModelTests
{
    [Test]
    public void Update_SnapsToPlayer_AndCollects()
    {
        var d = new ResourceDropModel(new Vector2(130, 0), DropType.Health, 10f);
        var player = Vector2.Zero;
        for (var i = 0; i < 80 && !d.Collected; i++)
            d.Update(player, 1f / 30f);

        Assert.That(d.Collected, Is.True);
    }

    [Test]
    public void Update_DoesNotMove_WhenAlreadyCollected()
    {
        var d = new ResourceDropModel(Vector2.Zero, DropType.Mana, 5f);
        d.Update(Vector2.Zero, 1f);
        Assert.That(d.Collected, Is.True);
        var before = d.Position;
        d.Update(new Vector2(999, 999), 1f);
        Assert.That(d.Position, Is.EqualTo(before));
    }
}

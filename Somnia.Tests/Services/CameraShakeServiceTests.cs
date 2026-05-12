using Somnia.Game.Models;
using Somnia.Game.Services.Camera;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class CameraShakeServiceTests
{
    [Test]
    public void Trigger_ClampsTrauma_ToOne()
    {
        var svc = new CameraShakeService(new Random(0));
        var cam = new CameraState();
        svc.Trigger(cam, 2.5f);
        Assert.That(cam.ShakeTrauma, Is.EqualTo(1f).Within(1e-3f));
    }

    [Test]
    public void Tick_DecaysTrauma_OverTime()
    {
        var svc = new CameraShakeService(new Random(0));
        var cam = new CameraState();
        svc.Trigger(cam, 1f);
        var before = cam.ShakeTrauma;
        svc.Tick(cam, 0.2f);
        Assert.That(cam.ShakeTrauma, Is.LessThan(before));
    }

    [Test]
    public void Tick_ProducesNonZeroOffset_WhenTraumaPositive()
    {
        var svc = new CameraShakeService(new Random(42));
        var cam = new CameraState();
        svc.Trigger(cam, 1f);
        svc.Tick(cam, 0.01f);
        Assert.That(cam.ShakeOffset.Length(), Is.GreaterThan(0f));
    }

    [Test]
    public void Tick_ReturnsZeroOffset_WhenTraumaZero()
    {
        var svc = new CameraShakeService(new Random(0));
        var cam = new CameraState();
        svc.Tick(cam, 0.1f);
        Assert.That(cam.ShakeOffset, Is.EqualTo(Vector2.Zero));
    }
}

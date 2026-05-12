using Somnia.Game.Services.Audio;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class AudioControllerVolumeTests
{
    [Test]
    public void Volume_IsClampedTo01()
    {
        var a = new AudioController();
        a.MasterVolume = 2.5f;
        a.MusicVolume = -3f;
        a.SfxVolume = 0.4f;
        Assert.That(a.MasterVolume, Is.EqualTo(1f));
        Assert.That(a.MusicVolume, Is.EqualTo(0f));
        Assert.That(a.SfxVolume, Is.EqualTo(0.4f).Within(1e-5));
    }

    [Test]
    public void Default_VolumesAreReasonable()
    {
        var a = new AudioController();
        Assert.That(a.MasterVolume, Is.InRange(0.5f, 1f));
        Assert.That(a.SfxVolume, Is.InRange(0.3f, 1f));
    }
}

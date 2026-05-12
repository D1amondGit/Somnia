using Somnia.Game.Models;
using Somnia.Game.Services.AI;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class EnemyAiServiceTests
{
    private static readonly Rectangle Area = new(0, 0, 1200, 800);

    [Test]
    public void Charger_RushesPlayer_AndExplodesOnContact()
    {
        var ai = new EnemyAiService();
        var player = new PlayerModel(new Vector2(100, 100));
        var npc = new NpcModel(new Vector2(900, 900));
        var charger = new EnemyModel(new Vector2(140, 100), EnemyType.Charger);
        var enemies = new List<EnemyModel> { charger };

        ai.Update(0.5f, enemies, player, npc, Area, new List<Vector3>(), new List<ProjectileModel>());

        Assert.That(charger.IsDead, Is.True, "charger must self-destruct on contact");
        Assert.That(player.CurrentHealth, Is.LessThan(player.MaxHealth));
    }

    [Test]
    public void Charger_Explosion_AccumulatesTrauma_ForCameraShake()
    {
        var ai = new EnemyAiService();
        var player = new PlayerModel(new Vector2(100, 100));
        var npc = new NpcModel(new Vector2(900, 900));
        var charger = new EnemyModel(new Vector2(120, 100), EnemyType.Charger);
        var enemies = new List<EnemyModel> { charger };

        ai.Update(0.5f, enemies, player, npc, Area, new List<Vector3>(), new List<ProjectileModel>());
        var trauma = ai.ConsumeTrauma();

        Assert.That(trauma, Is.GreaterThan(0f));
        Assert.That(ai.ConsumeTrauma(), Is.EqualTo(0f), "trauma must reset after consumption");
    }

    [Test]
    public void Sniper_ArmsTelegraph_BeforeShooting()
    {
        var ai = new EnemyAiService();
        var player = new PlayerModel(new Vector2(100, 400));
        var npc = new NpcModel(new Vector2(900, 900));
        var sniper = new EnemyModel(new Vector2(700, 400), EnemyType.Sniper);
        var enemies = new List<EnemyModel> { sniper };
        var projs = new List<ProjectileModel>();

        ai.Update(1f / 60f, enemies, player, npc, Area, new List<Vector3>(), projs);

        Assert.That(sniper.TelegraphArmed, Is.True);
        Assert.That(sniper.IsTelegraphing, Is.True);
        Assert.That(projs, Is.Empty, "sniper must not fire during telegraph");
    }

    [Test]
    public void Sniper_Fires_AfterTelegraphExpires()
    {
        var ai = new EnemyAiService();
        var player = new PlayerModel(new Vector2(100, 400));
        var npc = new NpcModel(new Vector2(900, 900));
        var sniper = new EnemyModel(new Vector2(700, 400), EnemyType.Sniper);
        var enemies = new List<EnemyModel> { sniper };
        var projs = new List<ProjectileModel>();

        ai.Update(1f / 60f, enemies, player, npc, Area, new List<Vector3>(), projs);
        // Промотаем фрейм, чтобы телеграф истёк
        sniper.Update(2f);
        ai.Update(1f / 60f, enemies, player, npc, Area, new List<Vector3>(), projs);

        Assert.That(projs.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(sniper.TelegraphArmed, Is.False);
        Assert.That(projs[0].Velocity.Length(),
            Is.EqualTo(EnemyArchetypeCatalog.Get(EnemyType.Sniper).ProjectileSpeed).Within(1f));
    }

    [Test]
    public void Melee_DealsArchetypeDamage_OnContact()
    {
        var ai = new EnemyAiService();
        var player = new PlayerModel(new Vector2(500, 500));
        var npc = new NpcModel(new Vector2(990, 990));
        var melee = new EnemyModel(new Vector2(530, 500), EnemyType.Melee);
        var enemies = new List<EnemyModel> { melee };

        var before = player.CurrentHealth;
        ai.Update(1f / 60f, enemies, player, npc, Area, new List<Vector3>(), new List<ProjectileModel>());
        Assert.That(player.CurrentHealth, Is.EqualTo(before - melee.Archetype.MeleeDamage).Within(1e-3f));
    }
}

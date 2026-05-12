using System.Collections.Generic;
using Somnia.Game.Models;
using Somnia.Game.Services.AI;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class BossControllerTests
{
    [Test]
    public void Boss_StartsIdle_AndPicksAttackAfterIdleTimer()
    {
        var boss = new EnemyModel(new Vector2(500, 500), EnemyType.Boss);
        var player = new PlayerModel(new Vector2(700, 500));
        var npc = new NpcModel(new Vector2(720, 520));

        // Доводим idle до конца — босс должен выбрать атаку.
        boss.BossPhaseTimer = 0.01f;
        BossController.Update(boss, player, npc, 200f, 0.05f,
            new List<ProjectileModel>(), null);

        Assert.That(boss.BossPhase, Is.Not.EqualTo(BossAttackPhase.Idle));
    }

    [Test]
    public void Boss_SlamHitsPlayer_WhenInsideRadius()
    {
        var boss = new EnemyModel(new Vector2(500, 500), EnemyType.Boss)
        {
            BossPhase = BossAttackPhase.SlamImpact,
            BossPhaseTimer = 0f,
            BossActionCenter = new Vector2(600, 500),
            BossActionRadius = 200f
        };
        var player = new PlayerModel(new Vector2(610, 500));
        var hpBefore = player.CurrentHealth;
        var npc = new NpcModel(new Vector2(5000, 5000)); // далеко

        BossController.Update(boss, player, npc, 0f, 0.016f,
            new List<ProjectileModel>(), null);

        Assert.That(player.CurrentHealth, Is.LessThan(hpBefore));
    }

    [Test]
    public void Boss_Volley_FiresMultipleProjectiles()
    {
        var boss = new EnemyModel(new Vector2(500, 500), EnemyType.Boss)
        {
            BossPhase = BossAttackPhase.VolleyFire,
            BossPhaseTimer = 0f,
            BossActionCenter = new Vector2(700, 500)
        };
        var player = new PlayerModel(new Vector2(700, 500));
        var projectiles = new List<ProjectileModel>();

        BossController.Update(boss, player, new NpcModel(Vector2.Zero), 200f, 0.016f,
            projectiles, null);

        Assert.That(projectiles.Count, Is.EqualTo(BossController.VolleyShots));
    }

    [Test]
    public void Boss_SlamBreaksDestructibleCover_InRange()
    {
        var boss = new EnemyModel(new Vector2(500, 500), EnemyType.Boss)
        {
            BossPhase = BossAttackPhase.SlamImpact,
            BossPhaseTimer = 0f,
            BossActionCenter = new Vector2(600, 500),
            BossActionRadius = 200f
        };
        var wall = new HexagonModel(new Vector2(620, 500), 60f)
        {
            MaxDestructibleHealth = 50f,
            DestructibleHealth = 50f
        };
        var destWalls = new List<HexagonModel> { wall };
        BossController.BrokenWallsThisFrame.Clear();

        BossController.Update(boss, new PlayerModel(new Vector2(5000, 5000)),
            new NpcModel(new Vector2(5000, 5000)), 200f, 0.016f,
            new List<ProjectileModel>(), destWalls);

        Assert.That(wall.IsBroken, Is.True);
        Assert.That(BossController.BrokenWallsThisFrame, Contains.Item(wall));
    }
}

using System.Collections.Generic;
using Somnia.Game.Models;
using Somnia.Game.Services.AI;

namespace Somnia.Tests.Services;

[TestFixture]
public sealed class EnemyClampTests
{
    [Test]
    public void Enemy_OutsidePlayArea_IsClampedBackIn()
    {
        // Враг выпихнут далеко за правый край — ИИ обязан вернуть его в playArea.
        var ai = new EnemyAiService();
        var enemy = new EnemyModel(new Vector2(5000, 5000), EnemyType.Melee);
        var enemies = new List<EnemyModel> { enemy };
        var player = new PlayerModel(new Vector2(400, 300));
        var npc = new NpcModel(new Vector2(420, 320));
        var playArea = new Rectangle(0, 0, 1280, 720);

        ai.Update(
            dt: 0.016f,
            enemies: enemies,
            player: player,
            npc: npc,
            playArea: playArea,
            walls: new List<Vector3>(),
            projectiles: new List<ProjectileModel>());

        Assert.That(enemy.Position.X, Is.LessThanOrEqualTo(playArea.Right));
        Assert.That(enemy.Position.X, Is.GreaterThanOrEqualTo(playArea.Left));
        Assert.That(enemy.Position.Y, Is.LessThanOrEqualTo(playArea.Bottom));
        Assert.That(enemy.Position.Y, Is.GreaterThanOrEqualTo(playArea.Top));
    }

    [Test]
    public void Stunned_Enemy_IsClampedToo()
    {
        var ai = new EnemyAiService();
        var enemy = new EnemyModel(new Vector2(-300, -300), EnemyType.Melee)
        {
            StunTimer = 1f
        };
        var enemies = new List<EnemyModel> { enemy };

        ai.Update(
            dt: 0.016f,
            enemies: enemies,
            player: new PlayerModel(new Vector2(400, 300)),
            npc: new NpcModel(new Vector2(420, 320)),
            playArea: new Rectangle(0, 0, 1280, 720),
            walls: new List<Vector3>(),
            projectiles: new List<ProjectileModel>());

        Assert.That(enemy.Position.X, Is.GreaterThanOrEqualTo(0));
        Assert.That(enemy.Position.Y, Is.GreaterThanOrEqualTo(0));
    }
}

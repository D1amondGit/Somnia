using Microsoft.Xna.Framework;
using NUnit.Framework;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Player_TakesDamage_HealthDecreases()
        {
            var player = new PlayerModel(Vector2.Zero);
            player.TakeDamage(20f);
            
            // В NUnit 4+ правильно использовать Assert.That
            Assert.That(player.CurrentHealth, Is.EqualTo(80f));
        }

        [Test]
        public void Player_Moves_PositionChanges()
        {
            var player = new PlayerModel(Vector2.Zero);
            // Двигаемся вправо 1 секунду. Базовая скорость 500.
            player.Move(new Vector2(1, 0), 1f, 2000, 2000);
            
            Assert.That(player.Position.X, Is.EqualTo(500f));
            Assert.That(player.Position.Y, Is.EqualTo(0f));
        }

        [Test]
        public void Player_CarryingState_MovesSlower()
        {
            var player = new PlayerModel(Vector2.Zero);
            player.SetState(PlayerState.Carrying);
            
            // Двигаемся вправо 1 секунду с грузом. Скорость падает до 250.
            player.Move(new Vector2(1, 0), 1f, 2000, 2000);
            
            Assert.That(player.Position.X, Is.EqualTo(250f));
        }
        
        [Test]
        public void Player_BlueZoneDash_MovesFaster()
        {
            var player = new PlayerModel(Vector2.Zero);
            
            // Настраиваем игрока для рывка (Синяя зона, 2-й слот)
            player.CurrentZone = AnomalyType.Blue;
            player.ActiveSlot = 1; // Индекс 1 = второй слот
            player.CurrentMana = 100f;
            
            // Активируем навык вправо
            player.UseActiveSkill(new Vector2(1, 0), new List<EnemyModel>());
            
            // Во время рывка скорость 2000. За 0.1 секунды пролетим 200 пикселей.
            player.Move(new Vector2(1, 0), 0.1f, 2000, 2000);
            
            Assert.That(player.Position.X, Is.EqualTo(200f));
        }
    }
}
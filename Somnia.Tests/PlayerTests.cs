using System.Numerics;
using NUnit.Framework;
using Somnia.Game.Models;

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
            
            Assert.Equals(80f, player.CurrentHealth);
        }

        [Test]
        public void Player_Moves_PositionChanges()
        {
            var player = new PlayerModel(Vector2.Zero);
            // Двигаемся вправо 1 секунду. Базовая скорость 500.
            player.Move(new Vector2(1, 0), 1f, 2000, 2000);
            
            Assert.Equals(500f, player.Position.X);
            Assert.Equals(0f, player.Position.Y);
        }

        [Test]
        public void Player_CarryingState_MovesSlower()
        {
            var player = new PlayerModel(Vector2.Zero);
            player.SetState(PlayerState.Carrying);
            
            // Двигаемся вправо 1 секунду с грузом. Скорость падает в 2 раза (до 250).
            player.Move(new Vector2(1, 0), 1f, 2000, 2000);
            
            Assert.Equals(250f, player.Position.X);
        }
        
        [Test]
        public void Player_Dash_MovesFaster()
        {
            var player = new PlayerModel(Vector2.Zero);
            player.StartDash(new Vector2(1, 0));
            
            // Во время рывка скорость х4 (2000). За 0.1 секунды пролетим 200 пикселей.
            player.Move(new Vector2(1, 0), 0.1f, 2000, 2000);
            
            Assert.Equals(200f, player.Position.X);
        }
    }
}
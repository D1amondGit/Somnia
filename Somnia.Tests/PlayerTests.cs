using System.Numerics;
using NUnit.Framework;
using Somnia.Game.Models; // Подключаем нашу игру к тестам

namespace Somnia.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Speed_ShouldBeHalved_WhenCarryingNPC()
        {
            // Arrange
            var player = new PlayerModel(Vector2.Zero);
            
            // Act
            player.SetState(PlayerState.Carrying);
            
            // Assert
            Assert.That(player.CurrentSpeed, Is.EqualTo(player.BaseSpeed * 0.5f));
        }

        [Test]
        public void Position_ShouldUpdateCorrectly_AfterMovement()
        {
            // Arrange
            var player = new PlayerModel(new Vector2(100, 100));
            var direction = new Vector2(1, 0); // Движение вправо
            float deltaTime = 1f; // 1 секунда

            // Act
            player.Move(direction, deltaTime, 1920, 1080);
            // Assert
            Assert.That(player.Position.X, Is.EqualTo(100 + player.BaseSpeed));
        }
    }
}
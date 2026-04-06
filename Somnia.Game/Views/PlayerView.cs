using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;

namespace Somnia.Game.Views
{
    public class PlayerView
    {
        private Texture2D _texture;

        public PlayerView(GraphicsDevice graphicsDevice)
        {
            _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.White });
        }

        public void DrawPlayer(SpriteBatch spriteBatch, PlayerModel model)
        {
            Color color = model.State == PlayerState.Free ? Color.Blue : Color.Green;
            spriteBatch.Draw(_texture, new Rectangle((int)model.Position.X, (int)model.Position.Y, 50, 50), color);
        }

        public void DrawPlayerUI(SpriteBatch spriteBatch, PlayerModel model, int screenWidth)
        {
            int barWidth = 200;
            int barHeight = 20;
            Rectangle healthBarBackground = new Rectangle(10, 10, barWidth, barHeight);
    
            int currentHealthWidth = (int)(barWidth * (model.CurrentHealth / model.MaxHealth));
            if (currentHealthWidth < 0) currentHealthWidth = 0; // Защита от минуса
            Rectangle healthBarForeground = new Rectangle(10, 10, currentHealthWidth, barHeight);

            spriteBatch.Draw(_texture, healthBarBackground, Color.Gray);
            spriteBatch.Draw(_texture, healthBarForeground, Color.Red);
        }

        public void DrawDamageZone(SpriteBatch spriteBatch, Rectangle damageZone)
        {
            spriteBatch.Draw(_texture, damageZone, Color.Red * 0.3f);
        }
        
        public void DrawNpc(SpriteBatch spriteBatch, NpcModel npc)
        {
            if (npc.IsPickedUp) return;

            // ВОТ ЭТА СТРОКА ПРОПАЛА: РИСУЕМ ТЕЛО NPC
            spriteBatch.Draw(_texture, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 40, 40), Color.Yellow);

            // Рисуем шкалу здоровья над NPC
            int barWidth = 60;
            int barHeight = 8;
            Vector2 barPosition = npc.Position + new Vector2(-10, -15); 

            Rectangle healthBarBackground = new Rectangle((int)barPosition.X, (int)barPosition.Y, barWidth, barHeight);
            int currentHealthWidth = (int)(barWidth * (npc.CurrentHealth / npc.MaxHealth));
            if (currentHealthWidth < 0) currentHealthWidth = 0;
            Rectangle healthBarForeground = new Rectangle((int)barPosition.X, (int)barPosition.Y, currentHealthWidth, barHeight);

            spriteBatch.Draw(_texture, healthBarBackground, Color.Gray);
            spriteBatch.Draw(_texture, healthBarForeground, Color.LimeGreen); // Сделал зеленым, чтобы отличалось от тела
        }
    }
}
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

        // КООРДИНАТЫ КНОПОК
        public Rectangle GetResumeButton(int screenWidth) => new Rectangle(screenWidth / 2 - 150, 300, 300, 60);
        public Rectangle GetExitButton(int screenWidth) => new Rectangle(screenWidth / 2 - 150, 400, 300, 60);
        public Rectangle GetRestartButton(int screenWidth) => new Rectangle(screenWidth / 2 - 150, 350, 300, 60);

        public void DrawPlayer(SpriteBatch spriteBatch, PlayerModel model)
        {
            if (model.IsDead) return;
    
            Color color;
            if (model.IsDashing) 
                color = Color.Cyan; // Цвет во время рывка
            else 
                color = model.State == PlayerState.Free ? Color.Blue : Color.Green;

            spriteBatch.Draw(_texture, new Rectangle((int)model.Position.X, (int)model.Position.Y, 50, 50), color);
        }

        public void DrawNpc(SpriteBatch spriteBatch, NpcModel npc)
        {
            if (npc.IsDead || npc.IsPickedUp) return; // Прячем, если мертв или на руках
            spriteBatch.Draw(_texture, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 40, 40), Color.Yellow);
            
            // Шкала здоровья NPC
            int barWidth = 60;
            int barHeight = 8;
            Vector2 barPosition = npc.Position + new Vector2(-10, -15); 
            Rectangle healthBarBackground = new Rectangle((int)barPosition.X, (int)barPosition.Y, barWidth, barHeight);
            int currentHealthWidth = (int)(barWidth * (npc.CurrentHealth / npc.MaxHealth));
            if (currentHealthWidth < 0) currentHealthWidth = 0;
            Rectangle healthBarForeground = new Rectangle((int)barPosition.X, (int)barPosition.Y, currentHealthWidth, barHeight);
            spriteBatch.Draw(_texture, healthBarBackground, Color.Gray);
            spriteBatch.Draw(_texture, healthBarForeground, Color.LimeGreen);
        }

        public void DrawPlayerUI(SpriteBatch spriteBatch, PlayerModel model)
        {
            if (model.IsDead) return; // Скрываем интерфейс при смерти
            int barWidth = 200;
            int barHeight = 20;
            Rectangle healthBarBackground = new Rectangle(10, 10, barWidth, barHeight);
            int currentHealthWidth = (int)(barWidth * (model.CurrentHealth / model.MaxHealth));
            if (currentHealthWidth < 0) currentHealthWidth = 0; 
            Rectangle healthBarForeground = new Rectangle(10, 10, currentHealthWidth, barHeight);
            spriteBatch.Draw(_texture, healthBarBackground, Color.Gray);
            spriteBatch.Draw(_texture, healthBarForeground, Color.Red);
        }

        public void DrawDamageZone(SpriteBatch spriteBatch, Rectangle damageZone)
        {
            spriteBatch.Draw(_texture, damageZone, Color.Red * 0.3f);
        }

        // ОТРИСОВКА ПАУЗЫ
        // ОТРИСОВКА ПАУЗЫ (ТЕПЕРЬ С ТЕКСТОМ)
        public void DrawPauseMenu(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight, Point mousePos)
        {
            spriteBatch.Draw(_texture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.6f);

            var resumeBtn = GetResumeButton(screenWidth);
            var exitBtn = GetExitButton(screenWidth);

            Color resumeColor = resumeBtn.Contains(mousePos) ? Color.LightGreen : Color.Green;
            Color exitColor = exitBtn.Contains(mousePos) ? Color.IndianRed : Color.Red;

            spriteBatch.Draw(_texture, resumeBtn, resumeColor);
            spriteBatch.Draw(_texture, exitBtn, exitColor);

            // Рисуем текст по центру кнопок
            DrawTextCentered(spriteBatch, font, "ПРОДОЛЖИТЬ", resumeBtn);
            DrawTextCentered(spriteBatch, font, "ВЫЙТИ", exitBtn);
        }

        // ОТРИСОВКА ЭКРАНА СМЕРТИ (ТЕПЕРЬ С ТЕКСТОМ)
        public void DrawGameOver(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight, Point mousePos)
        {
            spriteBatch.Draw(_texture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkRed * 0.8f);

            var restartBtn = GetRestartButton(screenWidth);
            Color restartColor = restartBtn.Contains(mousePos) ? Color.LightGreen : Color.Green;

            spriteBatch.Draw(_texture, restartBtn, restartColor);

            // Текст смерти и рестарта
            DrawTextCentered(spriteBatch, font, "ВЫ ПОГИБЛИ", new Rectangle(0, restartBtn.Y - 80, screenWidth, 50));
            DrawTextCentered(spriteBatch, font, "НАЧАТЬ ЗАНОВО", restartBtn);
        }

        // Вспомогательный метод для центрирования текста
        private void DrawTextCentered(SpriteBatch spriteBatch, SpriteFont font, string text, Rectangle container)
        {
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                container.X + (container.Width - textSize.X) / 2,
                container.Y + (container.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(font, text, textPos, Color.White);
        }
    }
}
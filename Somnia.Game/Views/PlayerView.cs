using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using System;

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
                color = Color.Cyan;
            else
                color = model.State == PlayerState.Free ? Color.Blue : Color.Green;

            spriteBatch.Draw(_texture, new Rectangle((int)model.Position.X, (int)model.Position.Y, 50, 50), color);
        }

        public void DrawNpc(SpriteBatch spriteBatch, NpcModel npc)
        {
            if (npc.IsDead || npc.IsPickedUp) return;
            spriteBatch.Draw(_texture, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 40, 40), Color.Yellow);
            DrawNpcHealthBar(spriteBatch, npc);
        }

        private void DrawNpcHealthBar(SpriteBatch spriteBatch, NpcModel npc)
        {
            int barWidth = 60;
            int barHeight = 8;
            Vector2 barPosition = npc.Position + new Vector2(-10, -15);
            Rectangle bg = new Rectangle((int)barPosition.X, (int)barPosition.Y, barWidth, barHeight);
            int fill = (int)(barWidth * (npc.CurrentHealth / npc.MaxHealth));
            fill = Math.Max(0, fill);
            Rectangle fg = new Rectangle((int)barPosition.X, (int)barPosition.Y, fill, barHeight);
            spriteBatch.Draw(_texture, bg, Color.Gray);
            spriteBatch.Draw(_texture, fg, Color.LimeGreen);
        }

        public void DrawPlayerUI(SpriteBatch spriteBatch, PlayerModel model)
        {
            if (model.IsDead) return;
            int barWidth = 200;
            int barHeight = 20;
            Rectangle bg = new Rectangle(10, 10, barWidth, barHeight);
            int fill = (int)(barWidth * (model.CurrentHealth / model.MaxHealth));
            fill = Math.Max(0, fill);
            Rectangle fg = new Rectangle(10, 10, fill, barHeight);
            spriteBatch.Draw(_texture, bg, Color.Gray);
            spriteBatch.Draw(_texture, fg, Color.Red);
        }

        public void DrawDamageZone(SpriteBatch spriteBatch, Rectangle damageZone)
        {
            spriteBatch.Draw(_texture, damageZone, Color.Red * 0.3f);
        }

        public void DrawAnomalyZone(SpriteBatch spriteBatch, AnomalyZone zone)
        {
            Color color = zone.Type == ZoneType.Red ? Color.Red * 0.25f : Color.Blue * 0.25f;
            spriteBatch.Draw(_texture, zone.Bounds, color);
        }

        public void DrawEnemy(SpriteBatch spriteBatch, EnemyModel enemy)
        {
            if (enemy.IsDead) return;
            spriteBatch.Draw(_texture,
                new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, 40, 40),
                Color.Purple);
            DrawEnemyHealthBar(spriteBatch, enemy);
        }

        private void DrawEnemyHealthBar(SpriteBatch sb, EnemyModel enemy)
        {
            int w = 40, h = 6;
            Vector2 pos = new Vector2(enemy.Position.X, enemy.Position.Y - 12);
            sb.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, w, h), Color.Gray);
            int fill = (int)(w * (enemy.CurrentHealth / enemy.MaxHealth));
            fill = Math.Max(0, fill);
            sb.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, fill, h), Color.LimeGreen);
        }

        public void DrawPlayerAttack(SpriteBatch spriteBatch, PlayerModel model)
        {
            if (!model.IsAttacking) return;
            Vector2 dir = GetFacingVector(model.FacingDirection);
            float range = model.GetAttackRange();
            int size = (int)(range * 0.5f);
            Vector2 center = model.Position + dir * range * 0.5f;
            int x = (int)center.X - size / 2;
            int y = (int)center.Y - size / 2;
            Color flash = GetAttackColor(model) * 0.4f;
            spriteBatch.Draw(_texture, new Rectangle(x, y, size, size), flash);
        }

        private static Vector2 GetFacingVector(FacingDirection dir)
        {
            return dir switch
            {
                FacingDirection.Up => new Vector2(0, -1),
                FacingDirection.Down => new Vector2(0, 1),
                FacingDirection.Left => new Vector2(-1, 0),
                _ => new Vector2(1, 0),
            };
        }

        private static Color GetAttackColor(PlayerModel model)
        {
            float dmg = model.GetAttackDamage();
            if (dmg >= 20f) return Color.OrangeRed;
            if (dmg <= 10f) return Color.LightBlue;
            return Color.Yellow;
        }

        public void DrawPauseMenu(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight, Point mousePos)
        {
            spriteBatch.Draw(_texture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.6f);
            var resumeBtn = GetResumeButton(screenWidth);
            var exitBtn = GetExitButton(screenWidth);
            Color resumeColor = resumeBtn.Contains(mousePos) ? Color.LightGreen : Color.Green;
            Color exitColor = exitBtn.Contains(mousePos) ? Color.IndianRed : Color.Red;
            spriteBatch.Draw(_texture, resumeBtn, resumeColor);
            spriteBatch.Draw(_texture, exitBtn, exitColor);
            DrawTextCentered(spriteBatch, font, "ПРОДОЛЖИТЬ", resumeBtn);
            DrawTextCentered(spriteBatch, font, "ВЫЙТИ", exitBtn);
        }

        public void DrawGameOver(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight, Point mousePos)
        {
            spriteBatch.Draw(_texture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkRed * 0.8f);
            var restartBtn = GetRestartButton(screenWidth);
            Color restartColor = restartBtn.Contains(mousePos) ? Color.LightGreen : Color.Green;
            spriteBatch.Draw(_texture, restartBtn, restartColor);
            DrawTextCentered(spriteBatch, font, "ВЫ ПОГИБЛИ", new Rectangle(0, restartBtn.Y - 80, screenWidth, 50));
            DrawTextCentered(spriteBatch, font, "НАЧАТЬ ЗАНОВО", restartBtn);
        }

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

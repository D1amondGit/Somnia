using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using System;

namespace Somnia.Game.Views
{
    public class PlayerView
    {
        private Texture2D _texture;

        public PlayerView(GraphicsDevice gd)
        {
            _texture = new Texture2D(gd, 1, 1);
            _texture.SetData(new[] { Color.White });
        }

        public Rectangle GetResumeButton(int sw) =>
            new Rectangle(sw / 2 - 150, 300, 300, 60);
        public Rectangle GetExitButton(int sw) =>
            new Rectangle(sw / 2 - 150, 400, 300, 60);
        public Rectangle GetRestartButton(int sw) =>
            new Rectangle(sw / 2 - 150, 350, 300, 60);

        public void DrawPlayer(SpriteBatch sb, PlayerModel m)
        {
            if (m.IsDead) return;
            Color c = m.IsDashing ? Color.Cyan
                : m.State == PlayerState.Free
                    ? Color.Blue : Color.Green;
            var r = new Rectangle(
                (int)m.Position.X, (int)m.Position.Y, 50, 50);
            sb.Draw(_texture, r, c);
        }

        public void DrawNpc(SpriteBatch sb, NpcModel npc)
        {
            if (npc.IsDead || npc.IsPickedUp) return;
            var r = new Rectangle(
                (int)npc.Position.X, (int)npc.Position.Y, 40, 40);
            sb.Draw(_texture, r, Color.Yellow);
            DrawNpcHealthBar(sb, npc);
        }

        private void DrawNpcHealthBar(SpriteBatch sb, NpcModel npc)
        {
            int w = 60, h = 8;
            Vector2 pos = npc.Position + new Vector2(-10, -15);
            var bg = new Rectangle((int)pos.X, (int)pos.Y, w, h);
            int fill = (int)(w * (npc.CurrentHealth / npc.MaxHealth));
            fill = Math.Max(0, fill);
            var fg = new Rectangle((int)pos.X, (int)pos.Y, fill, h);
            sb.Draw(_texture, bg, Color.Gray);
            sb.Draw(_texture, fg, Color.LimeGreen);
        }

        public void DrawPlayerUI(SpriteBatch sb, PlayerModel m)
        {
            if (m.IsDead) return;
            int w = 200, h = 20;
            var bg = new Rectangle(10, 10, w, h);
            int fill = (int)(w * (m.CurrentHealth / m.MaxHealth));
            fill = Math.Max(0, fill);
            var fg = new Rectangle(10, 10, fill, h);
            sb.Draw(_texture, bg, Color.Gray);
            sb.Draw(_texture, fg, Color.Red);
        }

        public void DrawDamageZone(SpriteBatch sb, Rectangle zone)
        {
            sb.Draw(_texture, zone, Color.Red * 0.3f);
        }

        public void DrawAnomalyZone(SpriteBatch sb, AnomalyZone zone)
        {
            Color c = GetZoneDrawColor(zone.Type);
            sb.Draw(_texture, zone.Bounds, c);
        }

        private static Color GetZoneDrawColor(ZoneType type)
        {
            return type switch
            {
                ZoneType.Red => Color.Red * 0.25f,
                ZoneType.Green => Color.Green * 0.25f,
                ZoneType.Blue => Color.Blue * 0.25f,
                _ => Color.White * 0.1f,
            };
        }

        public void DrawEnemy(SpriteBatch sb, EnemyModel e)
        {
            if (e.IsDead) return;
            var r = new Rectangle(
                (int)e.Position.X, (int)e.Position.Y, 40, 40);
            sb.Draw(_texture, r, Color.Purple);
            DrawEnemyHealthBar(sb, e);
        }

        private void DrawEnemyHealthBar(SpriteBatch sb, EnemyModel e)
        {
            int w = 40, h = 6;
            var pos = new Vector2(e.Position.X, e.Position.Y - 12);
            var bg = new Rectangle((int)pos.X, (int)pos.Y, w, h);
            sb.Draw(_texture, bg, Color.Gray);
            int fill = (int)(w * (e.CurrentHealth / e.MaxHealth));
            fill = Math.Max(0, fill);
            var fg = new Rectangle((int)pos.X, (int)pos.Y, fill, h);
            sb.Draw(_texture, fg, Color.LimeGreen);
        }

        public void DrawPlayerAttack(SpriteBatch sb, PlayerModel m)
        {
            if (!m.IsAttacking) return;
            float angle = GetAttackAngle(m.AttackDirection);
            float range = m.GetAttackRange();
            DrawAttackBeam(sb, m.Position, angle, range, m);
        }

        private void DrawAttackBeam(SpriteBatch sb, Vector2 pos,
            float angle, float range, PlayerModel m)
        {
            Color color = GetAttackColor(m) * 0.45f;
            int len = (int)range;
            int width = (int)(range * 0.5f);
            var rect = new Rectangle((int)pos.X, (int)pos.Y, len, width);
            var origin = new Vector2(0, width / 2f);
            sb.Draw(_texture, rect, null, color,
                angle, origin, SpriteEffects.None, 0f);
        }

        private static float GetAttackAngle(Vector2 dir)
        {
            return (float)Math.Atan2(dir.Y, dir.X);
        }

        private static Color GetAttackColor(PlayerModel m)
        {
            float dmg = m.GetAttackDamage();
            if (dmg >= 20f) return Color.OrangeRed;
            if (dmg <= 10f) return Color.LightBlue;
            return Color.Yellow;
        }

        public void DrawPauseMenu(SpriteBatch sb, SpriteFont font,
            int sw, int sh, Point mPos)
        {
            sb.Draw(_texture, new Rectangle(0, 0, sw, sh),
                Color.Black * 0.6f);
            var resume = GetResumeButton(sw);
            var exit = GetExitButton(sw);
            sb.Draw(_texture, resume, Hover(resume, mPos,
                Color.Green, Color.LightGreen));
            sb.Draw(_texture, exit, Hover(exit, mPos,
                Color.Red, Color.IndianRed));
            DrawTextCentered(sb, font, "ПРОДОЛЖИТЬ", resume);
            DrawTextCentered(sb, font, "ВЫЙТИ", exit);
        }

        private static Color Hover(Rectangle btn, Point mouse,
            Color normal, Color hover)
        {
            return btn.Contains(mouse) ? hover : normal;
        }

        public void DrawGameOver(SpriteBatch sb, SpriteFont font,
            int sw, int sh, Point mPos)
        {
            sb.Draw(_texture, new Rectangle(0, 0, sw, sh),
                Color.DarkRed * 0.8f);
            var btn = GetRestartButton(sw);
            Color c = btn.Contains(mPos) ? Color.LightGreen : Color.Green;
            sb.Draw(_texture, btn, c);
            DrawTextCentered(sb, font, "ВЫ ПОГИБЛИ",
                new Rectangle(0, btn.Y - 80, sw, 50));
            DrawTextCentered(sb, font, "НАЧАТЬ ЗАНОВО", btn);
        }

        private void DrawTextCentered(SpriteBatch sb, SpriteFont font,
            string text, Rectangle container)
        {
            Vector2 size = font.MeasureString(text);
            Vector2 pos = new Vector2(
                container.X + (container.Width - size.X) / 2,
                container.Y + (container.Height - size.Y) / 2);
            sb.DrawString(font, text, pos, Color.White);
        }
    }
}

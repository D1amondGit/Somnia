using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Views
{
    public class PlayerView
    {
        private Texture2D _tex;

        public PlayerView(GraphicsDevice gd)
        {
            _tex = new Texture2D(gd, 1, 1);
            _tex.SetData(new[] { Color.White });
        }

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies,
            List<AnomalyZone> zones, NpcModel npc, List<ResourceDropModel> drops, List<GateModel> gates)
        {
            foreach (var z in zones) sb.Draw(_tex, z.Bounds, GetZoneColor(z.Type) * 0.2f);
            foreach (var gate in gates) DrawGate(sb, gate);
            foreach (var drop in drops) DrawDrop(sb, drop);
            if (!npc.IsDead) DrawNpc(sb, npc);
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);

            Color pCol = p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green);
            sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), pCol);
            if (p.IsAttacking) DrawAttackEffect(sb, p);
        }

        private void DrawEnemy(SpriteBatch sb, EnemyModel e)
        {
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), Color.Purple);
            DrawBar(sb, new Rectangle((int)e.Position.X, (int)e.Position.Y - 10, 40, 5), e.Health, e.MaxHealth, Color.Red);
        }

        private void DrawNpc(SpriteBatch sb, NpcModel npc)
        {
            Color col = npc.IsInjured ? Color.OrangeRed : Color.Yellow;
            sb.Draw(_tex, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 30, 30), col);
            DrawBar(sb, new Rectangle((int)npc.Position.X, (int)npc.Position.Y - 10, 30, 5), npc.Health, npc.MaxHealth, Color.Lime);
        }

        private void DrawDrop(SpriteBatch sb, ResourceDropModel drop)
        {
            Color col = drop.Type == DropType.Health ? Color.Red : Color.Cyan;
            sb.Draw(_tex, new Rectangle((int)drop.Position.X, (int)drop.Position.Y, 12, 12), col);
        }

        private void DrawGate(SpriteBatch sb, GateModel gate)
        {
            Color col = gate.IsOpen ? Color.Lime * 0.5f : Color.DarkRed;
            sb.Draw(_tex, new Rectangle((int)gate.Position.X - 10, (int)gate.Position.Y - 40, 20, 80), col);
        }

        private void DrawAttackEffect(SpriteBatch sb, PlayerModel p)
        {
            var effectPos = p.Position + p.FacingDirection * 40f;
            sb.Draw(_tex, new Rectangle((int)effectPos.X, (int)effectPos.Y, 30, 30), Color.Orange * 0.6f);
        }

        private void DrawBar(SpriteBatch sb, Rectangle area, float current, float max, Color fillColor)
        {
            sb.Draw(_tex, area, Color.Gray);
            sb.Draw(_tex, new Rectangle(area.X, area.Y, (int)(area.Width * (current / max)), area.Height), fillColor);
        }

        public void DrawUI(SpriteBatch sb, PlayerModel p, NpcModel npc, SpriteFont font, int screenWidth)
        {
            DrawBar(sb, new Rectangle(20, 20, 200, 20), p.CurrentHealth, p.MaxHealth, Color.Red);
            DrawBar(sb, new Rectangle(20, 48, 120, 12), npc.Health, npc.MaxHealth, Color.Lime);
            sb.DrawString(font, $"ZONE: {p.CurrentZone}", new Vector2(20, 68), Color.White);
            if (p.DamageMultiplier < 1f)
                sb.DrawString(font, "NPC INJURED - DMG -50%", new Vector2(20, 90), Color.OrangeRed);
        }

        private Color GetZoneColor(AnomalyType t)
            => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : Color.Green);
    }
}

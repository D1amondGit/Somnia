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

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies, List<AnomalyZone> zones, NpcModel npc)
        {
            foreach (var z in zones) sb.Draw(_tex, z.Area, GetZoneColor(z.Type) * 0.2f);
            if (npc != null && !npc.IsPickedUp) DrawNpc(sb, npc);
            
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);
            
            Color pCol = p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green);
            sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), pCol);
        }

        private void DrawNpc(SpriteBatch sb, NpcModel npc)
        {
            sb.Draw(_tex, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 40, 40), Color.Yellow);
            sb.Draw(_tex, new Rectangle((int)npc.Position.X, (int)npc.Position.Y - 10, (int)(40 * (npc.Health/100f)), 5), Color.LimeGreen);
        }

        private void DrawEnemy(SpriteBatch sb, EnemyModel e)
        {
            Color eCol = e.StunTimer > 0 ? Color.White : (e.SlowTimer > 0 ? Color.CornflowerBlue : Color.Purple);
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), eCol);
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y - 10, (int)(40 * (e.Health/e.MaxHealth)), 5), Color.Red);
        }

        public void DrawUI(SpriteBatch sb, PlayerModel p, SpriteFont font, int screenW, int screenH)
        {
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth/p.MaxHealth)), 20), Color.Red);
            int uiX = screenW - 100; int uiY = screenH - 120;
            
            sb.Draw(_tex, new Rectangle(uiX, uiY, 60, 60), GetZoneColor(p.CurrentZone));
            
            float currentCd = p.ActiveSlot == 0 ? p.Cd1 : (p.ActiveSlot == 1 ? p.Cd2 : p.Cd3);
            float maxCd = p.ActiveSlot == 0 ? p.MaxCd1 : (p.ActiveSlot == 1 ? p.MaxCd2 : p.MaxCd3);
            
            float cdPct = maxCd > 0 ? currentCd / maxCd : 0;
            sb.Draw(_tex, new Rectangle(uiX, uiY, 60, (int)(60 * cdPct)), Color.Black * 0.7f);
            
            sb.Draw(_tex, new Rectangle(uiX, uiY + 70, 60, 10), Color.DarkBlue);
            sb.Draw(_tex, new Rectangle(uiX, uiY + 70, (int)(60 * (p.CurrentMana/100f)), 10), Color.Cyan);

            if (font != null) sb.DrawString(font, $"SLOT: {p.ActiveSlot + 1}", new Vector2(uiX, uiY - 30), Color.White);
        }
        
        public void DrawPauseMenu(SpriteBatch sb, SpriteFont font, int w, int h)
        {
            sb.Draw(_tex, new Rectangle(0, 0, w, h), Color.Black * 0.6f);
            if (font != null) 
            {
                sb.DrawString(font, "=== PAUSED ===", new Vector2(w / 2 - 60, h / 2 - 50), Color.White);
                sb.DrawString(font, "Press ESC to Resume", new Vector2(w / 2 - 80, h / 2), Color.LightGray);
                sb.DrawString(font, "Press R to Restart", new Vector2(w / 2 - 75, h / 2 + 30), Color.LightGray);
            }
        }

        private Color GetZoneColor(AnomalyType t) => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : Color.Green);
    }
}
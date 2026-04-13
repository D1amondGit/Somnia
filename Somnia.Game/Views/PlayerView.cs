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

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies, List<AnomalyZone> zones)
        {
            foreach (var z in zones) sb.Draw(_tex, z.Area, GetZoneColor(z.Type) * 0.2f);
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);
            
            Color pCol = p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green);
            sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), pCol);
            
            if (p.IsAttacking) DrawAttackEffect(sb, p);
        }

        private void DrawEnemy(SpriteBatch sb, EnemyModel e)
        {
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), Color.Purple);
            Rectangle hpBack = new Rectangle((int)e.Position.X, (int)e.Position.Y - 10, 40, 5);
            sb.Draw(_tex, hpBack, Color.Gray);
            sb.Draw(_tex, new Rectangle(hpBack.X, hpBack.Y, (int)(40 * (e.Health/e.MaxHealth)), 5), Color.Red);
        }

        private void DrawAttackEffect(SpriteBatch sb, PlayerModel p)
        {
            Vector2 effectPos = p.Position + p.FacingDirection * 40;
            sb.Draw(_tex, new Rectangle((int)effectPos.X, (int)effectPos.Y, 30, 30), Color.Orange * 0.8f);
        }

        public void DrawUI(SpriteBatch sb, PlayerModel p, SpriteFont font)
        {
            Rectangle bar = new Rectangle(20, 20, 200, 20);
            sb.Draw(_tex, bar, Color.Gray);
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth/p.MaxHealth)), 20), Color.Red);
            
            if (font != null)
                sb.DrawString(font, $"ZONE: {p.CurrentZone}", new Vector2(20, 50), Color.White);
        }

        private Color GetZoneColor(AnomalyType t) => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : Color.LightGray);
    }
}
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using System.Collections.Generic;

namespace Somnia.Game.Views
{
    public class PlayerView
    {
        private Texture2D _tex;
        public PlayerView(GraphicsDevice gd) { _tex = new Texture2D(gd, 1, 1); _tex.SetData(new[] { Color.White }); }

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies, List<AnomalyZone> zones, NpcModel npc, Rectangle hatch, Rectangle playArea, List<HexagonModel> walls, Texture2D wallTex, List<ProjectileModel> projs)
        {
            // Рисуем зоны
            foreach (var z in zones) sb.Draw(_tex, z.Area, GetZoneColor(z.Type) * 0.25f);
            
            // Сначала стены всех гексагонов
            foreach(var w in walls) DrawHexWalls(sb, w, wallTex);
            // Затем черные крыши
            foreach(var w in walls) FillPoly(sb, w.GetTopVertices(), Color.Black);

            // ФИКС: Рисуем черные границы поверх гексагонов, чтобы скрыть "хвосты" за экраном
            DrawBorders(sb, playArea);

            Color hCol = p.State == PlayerState.Carrying ? Color.Cyan : Color.DarkBlue;
            FillPoly(sb, new HexagonModel(new Vector2(hatch.X + hatch.Width/2, hatch.Y + hatch.Height/2), 40f).GetBaseVertices(), hCol); 

            foreach (var pr in projs) DrawCircle(sb, pr.Position, 8f, Color.Red, 8);
            if (npc != null && !npc.IsPickedUp) DrawNpc(sb, npc);
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);
            DrawPlayer(sb, p);
        }

        private void DrawHexWalls(SpriteBatch sb, HexagonModel hex, Texture2D tex) {
            if (tex == null) return;
            var r = hex.GetTopVertices();
            DrawWall(sb, tex, r.ElementAt(0), r.ElementAt(1), hex.WallHeight);
            DrawWall(sb, tex, r.ElementAt(1), r.ElementAt(2), hex.WallHeight);
            DrawWall(sb, tex, r.ElementAt(2), r.ElementAt(3), hex.WallHeight);
        }

        private void DrawWall(SpriteBatch sb, Texture2D tex, Vector2 p1, Vector2 p2, float h) {
            if (p1.X > p2.X) { var t = p1; p1 = p2; p2 = t; } 
            float w = p2.X - p1.X; if (w <= 0) return;
            for (float x = 0; x <= w; x += 1f) {
                float ty = MathHelper.Lerp(p1.Y, p2.Y, x / w);
                sb.Draw(tex, new Rectangle((int)Math.Round(p1.X + x), (int)Math.Round(ty), 2, (int)Math.Round(h)), 
                       new Rectangle((int)(p1.X + x) % tex.Width, 0, 1, tex.Height), Color.Gray);
            }
        }

        private void FillPoly(SpriteBatch sb, List<Vector2> v, Color c) {
            float minY = v.Min(p => p.Y), maxY = v.Max(p => p.Y);
            for (float y = minY; y <= maxY; y += 1f) {
                var xNodes = new List<float>();
                for (int i = 0; i < 6; i++) {
                    Vector2 p1 = v.ElementAt(i), p2 = v.ElementAt((i + 1) % 6);
                    if ((p1.Y < y && p2.Y >= y) || (p2.Y < y && p1.Y >= y))
                        xNodes.Add(p1.X + (y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X));
                }
                xNodes.Sort();
                if (xNodes.Count >= 2) {
                    float x1 = xNodes.ElementAt(0), x2 = xNodes.ElementAt(1);
                    sb.Draw(_tex, new Rectangle((int)x1, (int)y, (int)(x2 - x1 + 1), 2), c);
                }
            }
        }

        private void DrawBorders(SpriteBatch sb, Rectangle p) {
            int sw = sb.GraphicsDevice.Viewport.Width;
            int sh = sb.GraphicsDevice.Viewport.Height;

            // Теперь черные плашки начинаются ЗА визуальной границей экрана,
            // чтобы гексагоны были видны полностью, а за ними была тьма
            sb.Draw(_tex, new Rectangle(0, -1000, sw, 950), Color.Black); // Сверху (заканчивается на -50)
            sb.Draw(_tex, new Rectangle(0, sh + 50, sw, 1000), Color.Black); // Снизу (начинается на +50)
            sb.Draw(_tex, new Rectangle(-1000, 0, 950, sh), Color.Black); // Слева
            sb.Draw(_tex, new Rectangle(sw + 50, 0, 1000, sh), Color.Black); // Справа
        }

        private void DrawPlayer(SpriteBatch sb, PlayerModel p) {
            Color c = p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green);
            sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), c);
        }

        private void DrawNpc(SpriteBatch sb, NpcModel n) => sb.Draw(_tex, new Rectangle((int)n.Position.X, (int)n.Position.Y, 40, 40), Color.Yellow);
        private void DrawEnemy(SpriteBatch sb, EnemyModel e) => sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), Color.Purple);
        private Color GetZoneColor(AnomalyType t) => t switch {
            AnomalyType.Red => Color.Red,
            AnomalyType.Blue => Color.Blue,
            AnomalyType.Green => Color.LimeGreen,
            AnomalyType.Neutral => Color.Gray * 0.5f,
            _ => Color.White
        };
        
        public void DrawUI(SpriteBatch sb, PlayerModel p, SpriteFont f, int w, int h, int lvl) {
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth/p.MaxHealth)), 20), Color.Red);
            if (f != null) sb.DrawString(f, $"SLOT: {p.ActiveSlot + 1}", new Vector2(w - 150, h - 50), Color.White);
        }

        private void DrawCircle(SpriteBatch sb, Vector2 center, float r, Color c, int t=2) {
            float inc = MathHelper.TwoPi / 32f; float th = 0f;
            Vector2 p1 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * r;
            for (int i = 0; i < 32; i++) {
                th += inc; Vector2 p2 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * r;
                Vector2 e = p2 - p1; float a = (float)Math.Atan2(e.Y, e.X);
                sb.Draw(_tex, new Rectangle((int)p1.X, (int)p1.Y, (int)e.Length(), t), null, c, a, new Vector2(0, 0.5f), 0, 0);
                p1 = p2;
            }
        }
    }
}
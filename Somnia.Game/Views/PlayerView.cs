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

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies, List<AnomalyZone> zones, NpcModel npc, Rectangle hatch, Rectangle playArea, List<HexagonModel> walls, Texture2D wallTex, List<ProjectileModel> projs, List<ResourceDropModel> drops, List<GateModel> gates)
        {
            foreach (var g in gates) {
                var h = new HexagonModel(g.Position, 120f, 70f, 0.7f, 0.04f);
                FillPoly(sb, h.GetTopVertices(), g.IsOpen ? Color.LimeGreen : Color.Red * 0.5f);
            }

            DrawBorders(sb, playArea);
            DrawPlayer(sb, p);
            foreach (var e in enemies.Where(x => !x.IsDead)) DrawEnemy(sb, e);
            if (npc != null && !npc.IsPickedUp) DrawNpc(sb, npc);
        }

        private void DrawHexWalls(SpriteBatch sb, HexagonModel hex, Texture2D tex)
        {
            if (tex == null) return;
            var r = hex.GetTopVertices();
            DrawWall(sb, tex, r.ElementAt(0), r.ElementAt(1), hex.WallHeight);
            DrawWall(sb, tex, r.ElementAt(1), r.ElementAt(2), hex.WallHeight);
            DrawWall(sb, tex, r.ElementAt(2), r.ElementAt(3), hex.WallHeight);
        }

        private void FillPoly(SpriteBatch sb, List<Vector2> v, Color c)
        {
            float minY = v.Min(p => p.Y), maxY = v.Max(p => p.Y);
            for (float y = minY; y <= maxY; y += 1f) {
                var nodes = new List<float>();
                for (int i = 0; i < v.Count; i++) {
                    Vector2 p1 = v.ElementAt(i); Vector2 p2 = v.ElementAt((i + 1) % v.Count);
                    if ((p1.Y < y && p2.Y >= y) || (p2.Y < y && p1.Y >= y))
                        nodes.Add(p1.X + (y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X));
                }
                nodes.Sort();
                if (nodes.Count >= 2) sb.Draw(_tex, new Rectangle((int)nodes.ElementAt(0), (int)y, (int)(nodes.ElementAt(1) - nodes.ElementAt(0) + 1), 2), c);
            }
        }

        private void DrawWall(SpriteBatch sb, Texture2D tex, Vector2 p1, Vector2 p2, float h)
        {
            if (p1.X > p2.X) { var t = p1; p1 = p2; p2 = t; }
            float w = p2.X - p1.X; if (w <= 0) return;
            for (float x = 0; x <= w; x++) {
                float ty = MathHelper.Lerp(p1.Y, p2.Y, x / w);
                sb.Draw(tex, new Rectangle((int)(p1.X + x), (int)ty, 2, (int)h), Color.Gray);
            }
        }


        public void DrawUI(SpriteBatch sb, PlayerModel p, NpcModel npc, SpriteFont f, int w, int h)
        {
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth / p.MaxHealth)), 20), Color.Red);
            if (npc != null && npc.Health < 50f && f != null)
                sb.DrawString(f, "NPC INJURED - DMG -50%", new Vector2(w / 2 - 100, 20), Color.Orange);
        }

        private void DrawBorders(SpriteBatch sb, Rectangle p) { /* Твой старый DrawBorders */ }
        private void DrawPlayer(SpriteBatch sb, PlayerModel p) => sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), Color.Blue);
        private void DrawNpc(SpriteBatch sb, NpcModel n) => sb.Draw(_tex, new Rectangle((int)n.Position.X, (int)n.Position.Y, 40, 40), Color.Yellow);
        private void DrawEnemy(SpriteBatch sb, EnemyModel e) => sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), Color.Purple);
        private Color GetZoneColor(AnomalyType t) => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : (t == AnomalyType.Green ? Color.Green : Color.Gray));
        private void DrawCircle(SpriteBatch sb, Vector2 center, float r, Color c, int t) { /* Твой старый DrawCircle */ }
    }
}
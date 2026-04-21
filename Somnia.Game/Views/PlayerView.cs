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
        
        public PlayerView(GraphicsDevice gd) 
        { 
            _tex = new Texture2D(gd, 1, 1); 
            _tex.SetData(new List<Color> { Color.White }.ToArray()); 
        }

        public void DrawWorld(SpriteBatch sb, PlayerModel p, List<EnemyModel> enemies, List<AnomalyZone> zones, NpcModel npc, Rectangle playArea, List<HexagonModel> walls, Texture2D wallTex, List<ResourceDropModel> drops, List<GateModel> gates, List<FloatingText> texts, SpriteFont font, List<ProjectileModel> projs)
        {
            foreach (var z in zones) {
                var h = new HexagonModel(z.Center, z.Radius, 0f, 0.7f, 0.04f);
                FillPoly(sb, h.GetBaseVertices(), GetZoneColor(z.Type) * 0.15f);
            }
            foreach (var w in walls) DrawHexWalls(sb, w, wallTex);
            foreach (var w in walls) FillPoly(sb, w.GetTopVertices(), Color.Black);
            foreach (var g in gates) {
                var h = new HexagonModel(g.Position, 80f, 40f, 0.7f, 0.04f);
                FillPoly(sb, h.GetTopVertices(), g.IsOpen ? Color.LimeGreen : Color.Red * 0.5f);
            }
            
            DrawBorders(sb, playArea); 
            foreach (var d in drops) DrawCircle(sb, d.Position, 6f, d.Type == DropType.Health ? Color.Red : Color.Cyan, 5);
            foreach (var pr in projs) DrawCircle(sb, pr.Position, pr.Radius, Color.Orange, 4); // Отрисовка снарядов стрелков
            
            DrawPlayer(sb, p);
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);
            if (npc != null && !npc.IsPickedUp && !npc.IsDead) DrawNpc(sb, npc);
            
            if (p.GreenAuraTimer > 0) DrawCircle(sb, p.Position, 200f, Color.LimeGreen, 3);
            if (p.IsAttacking) DrawAttackEffect(sb, p);

            if (font != null) foreach (var t in texts) sb.DrawString(font, t.Text, t.Position + new Vector2(-20, -50), t.Color * t.Timer);
        }

        private void DrawAttackEffect(SpriteBatch sb, PlayerModel p)
        {
            Vector2 cPos = p.Position; Color c = GetZoneColor(p.CurrentZone);
            if (p.CurrentZone == AnomalyType.Neutral && p.ActiveSlot == 0) DrawLine(sb, cPos, cPos + p.FacingDir * 800f, Color.White, 8); 
            else if (p.CurrentZone == AnomalyType.Green && p.ActiveSlot == 0) DrawLine(sb, cPos, cPos + p.FacingDir * 1000f, Color.Lime, 15);
            else if (p.CurrentZone == AnomalyType.Red && p.ActiveSlot == 0) DrawCone(sb, cPos, p.FacingDir, 250f, 0.6f, Color.Red);
            else if (p.CurrentZone == AnomalyType.Red && p.ActiveSlot == 2) DrawCircle(sb, cPos, 300f, Color.Red, 10);
            else if (p.CurrentZone == AnomalyType.Blue && p.ActiveSlot == 1) DrawCircle(sb, cPos, 250f, Color.Blue, 5);
        }

        private void DrawBorders(SpriteBatch sb, Rectangle p)
        {
            sb.Draw(_tex, new Rectangle(-2000, -2000, 8000, p.Y + 2000), Color.Black); 
            sb.Draw(_tex, new Rectangle(-2000, p.Bottom, 8000, 2000), Color.Black); 
            sb.Draw(_tex, new Rectangle(-2000, p.Y, p.X + 2000, p.Height), Color.Black); 
            sb.Draw(_tex, new Rectangle(p.Right, p.Y, 2000, p.Height), Color.Black); 
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
                    if ((p1.Y < y && p2.Y >= y) || (p2.Y < y && p1.Y >= y)) nodes.Add(p1.X + (y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X));
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

        public void DrawUI(SpriteBatch sb, PlayerModel p, NpcModel npc, SpriteFont f, int w, int h, int state, int arenaNum)
        {
            sb.Draw(_tex, new Rectangle(10, 10, 220, 80), Color.Black * 0.7f); 
            sb.Draw(_tex, new Rectangle(20, 20, 200, 20), Color.DarkRed);
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth / p.MaxHealth)), 20), Color.Red);
            sb.Draw(_tex, new Rectangle(20, 50, 200, 10), Color.DarkBlue);
            sb.Draw(_tex, new Rectangle(20, 50, (int)(200 * (p.CurrentMana / 100f)), 10), Color.Cyan);
            
            int uX = w - 100; int uY = h - 120;
            sb.Draw(_tex, new Rectangle(uX, uY, 60, 60), GetZoneColor(p.CurrentZone));
            float cCd = p.ActiveSlot == 0 ? p.Cd1 : (p.ActiveSlot == 1 ? p.Cd2 : p.Cd3);
            float mCd = p.ActiveSlot == 0 ? p.MaxCd1 : (p.ActiveSlot == 1 ? p.MaxCd2 : p.MaxCd3);
            float pCd = mCd > 0 ? cCd / mCd : 0;
            
            int cdH = (int)(60 * pCd);
            sb.Draw(_tex, new Rectangle(uX, uY + 60 - cdH, 60, cdH), Color.Black * 0.8f);
            
            if (f != null) {
                sb.DrawString(f, $"ARENA: {arenaNum} / 3", new Vector2(uX - 20, uY - 60), Color.Gold);
                sb.DrawString(f, $"SLOT: {p.ActiveSlot + 1} ZONE: {p.CurrentZone}", new Vector2(20, 70), Color.White);
                sb.DrawString(f, $"CD: {cCd:F1}s", new Vector2(uX - 10, uY - 30), Color.White);
                if (npc != null && npc.Health < 50f) sb.DrawString(f, "!!! NPC INJURED - DMG -50% !!!", new Vector2(w / 2 - 150, 20), Color.OrangeRed);
                DrawStateText(sb, f, w, h, state);
            }
        }

        private void DrawStateText(SpriteBatch sb, SpriteFont f, int w, int h, int state)
        {
            if (state == 0) return;
            sb.Draw(_tex, new Rectangle(0, 0, w, h), Color.Black * 0.6f);
            if (state == 1) {
                sb.DrawString(f, "=== PAUSED ===", new Vector2(w/2 - 60, h/2 - 50), Color.White);
                sb.DrawString(f, "Press ESC to Resume", new Vector2(w/2 - 80, h/2), Color.LightGray);
            } else {
                sb.DrawString(f, "=== GAME OVER ===", new Vector2(w/2 - 70, h/2 - 50), Color.Red);
                sb.DrawString(f, "Press ENTER to Restart", new Vector2(w/2 - 85, h/2), Color.LightGray);
            }
        }

        private void DrawPlayer(SpriteBatch sb, PlayerModel p) => sb.Draw(_tex, new Rectangle((int)p.Position.X - 25, (int)p.Position.Y - 25, 50, 50), p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green));
        
        private void DrawNpc(SpriteBatch sb, NpcModel npc)
        {
            sb.Draw(_tex, new Rectangle((int)npc.Position.X - 20, (int)npc.Position.Y - 20, 40, 40), Color.Yellow);
            sb.Draw(_tex, new Rectangle((int)npc.Position.X - 20, (int)npc.Position.Y - 30, (int)(40 * (npc.Health/100f)), 5), Color.LimeGreen);
        }

        private void DrawEnemy(SpriteBatch sb, EnemyModel e)
        {
            Color eCol = e.DamageFlash > 0 ? Color.White : (e.StunTimer > 0 ? Color.LightGray : (e.SlowTimer > 0 ? Color.CornflowerBlue : Color.Purple));
            sb.Draw(_tex, new Rectangle((int)e.Position.X - 20, (int)e.Position.Y - 20, 40, 40), eCol);
            sb.Draw(_tex, new Rectangle((int)e.Position.X - 20, (int)e.Position.Y - 30, (int)(40 * (e.Health/e.MaxHealth)), 5), Color.Red);
        }

        private Color GetZoneColor(AnomalyType t) => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : (t == AnomalyType.Green ? Color.Green : Color.Gray));

        private void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c, int t=2) {
            Vector2 e = p2 - p1; float a = (float)Math.Atan2(e.Y, e.X);
            sb.Draw(_tex, new Rectangle((int)p1.X, (int)p1.Y, (int)e.Length(), t), null, c, a, new Vector2(0, 0.5f), 0, 0);
        }

        private void DrawCircle(SpriteBatch sb, Vector2 center, float r, Color c, int t=2) {
            float inc = MathHelper.TwoPi / 32f; float th = 0f;
            Vector2 p1 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * r;
            for (int i = 0; i < 32; i++) {
                th += inc; Vector2 p2 = center + new Vector2((float)Math.Cos(th), (float)Math.Sin(th)) * r;
                DrawLine(sb, p1, p2, c, t); p1 = p2;
            }
        }
        
        private void DrawCone(SpriteBatch sb, Vector2 c, Vector2 d, float r, float aStr, Color col) {
            float bA = (float)Math.Atan2(d.Y, d.X); float s = (float)Math.Acos(aStr);
            Vector2 p1 = c + new Vector2((float)Math.Cos(bA-s), (float)Math.Sin(bA-s)) * r;
            Vector2 p2 = c + new Vector2((float)Math.Cos(bA+s), (float)Math.Sin(bA+s)) * r;
            DrawLine(sb, c, p1, col, 3); DrawLine(sb, c, p2, col, 3);
            DrawLine(sb, p1, c + d * r, col, 2); DrawLine(sb, c + d * r, p2, col, 2);
        }
    }
}
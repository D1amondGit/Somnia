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
            foreach (var z in zones) sb.Draw(_tex, z.Area, GetZoneColor(z.Type) * 0.25f);
            sb.Draw(_tex, new Rectangle(0, 0, 1280, playArea.Y), Color.Black); 
            sb.Draw(_tex, new Rectangle(0, playArea.Bottom, 1280, 720 - playArea.Bottom), Color.Black); 
            sb.Draw(_tex, new Rectangle(0, playArea.Y, playArea.X, playArea.Height), Color.Black); 
            sb.Draw(_tex, new Rectangle(playArea.Right, playArea.Y, 1280 - playArea.Right, playArea.Height), Color.Black); 
            
            // 1. СТЕНЫ (Текстура)
            foreach(var w in walls) DrawHexWalls(sb, w, wallTex);
            
            // 2. КРЫШИ (Черные, поверх стен)
            foreach(var w in walls) FillHexagon(sb, w.Center - new Vector2(0, w.WallHeight), w.Radius, w.SquashFactor, Color.Black);

            Color hCol = p.State == PlayerState.Carrying ? Color.Cyan : Color.DarkBlue;
            FillHexagon(sb, new Vector2(hatch.X + hatch.Width/2, hatch.Y + hatch.Height/2), 40f, 0.5f, hCol); 

            foreach (var pr in projs) DrawCircle(sb, pr.Position, 8f, Color.Red, 8);
            if (npc != null && !npc.IsPickedUp) DrawNpc(sb, npc);
            foreach (var e in enemies) if (!e.IsDead) DrawEnemy(sb, e);
            
            Color pCol = p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green);
            sb.Draw(_tex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 50, 50), pCol);
            
            if (p.GreenAuraTimer > 0) DrawCircle(sb, p.Position + new Vector2(25, 25), 200f, Color.LimeGreen, 3);
            if (p.IsAttacking) DrawAttackEffect(sb, p);
        }

        private void DrawHexWalls(SpriteBatch sb, HexagonModel hex, Texture2D tex) {
            if (tex == null) return;
            Vector2[] r = hex.GetTopVertices(); 
            
            // Для Pointy-topped гексагона видимыми передними гранями являются (в изометрии):
            // Грань 2-1 (передняя-левая), Грань 1-0 (нижняя), Грань 0-5 (передняя-правая)
            DrawWall(sb, tex, r, r, hex.WallHeight);
            DrawWall(sb, tex, r, r, hex.WallHeight);
            DrawWall(sb, tex, r, r, hex.WallHeight);
        }

        private void DrawWall(SpriteBatch sb, Texture2D tex, Vector2 p1, Vector2 p2, float h) {
            if (p1.X > p2.X) { var t = p1; p1 = p2; p2 = t; } 
            float w = p2.X - p1.X;
            if (w <= 0) return;

            for (float x = 0; x <= w; x += 1f) {
                float topY = MathHelper.Lerp(p1.Y, p2.Y, x / w);
                // Ширина 2 пикселя спасает от микро-щелей
                Rectangle d = new Rectangle((int)Math.Round(p1.X + x), (int)Math.Round(topY), 2, (int)Math.Round(h));
                
                int srcX = (int)(p1.X + x) % tex.Width;
                if (srcX < 0) srcX += tex.Width;
                Rectangle s = new Rectangle(srcX, 0, 1, tex.Height);
                
                sb.Draw(tex, d, s, Color.Gray);
            }
        }

        // --- ИСПРАВЛЕННАЯ МАТЕМАТИКА ЗАЛИВКИ Pointy-topped ГЕКСАГОНА ---
        private void FillHexagon(SpriteBatch sb, Vector2 c, float r, float sq, Color col) {
            // Pointy-topped гексагон: высота по Y = R, ширина по X = R * 0.866 * 2
            float s32 = 0.8660254f; // sin(60)
            float maxH = r * sq; // Высота от центра до точки (сплющенная)
            
            for (float y = -maxH; y <= maxH; y += 1f) {
                // Восстанавливаем "несплющенный" Y для расчета ширины
                float unsquashedY = y / sq;
                
                // Формула ширины правильного Pointy-topped гексагона:
                // w = (R - |y| / sin(60)) * 2
                float w = (r - Math.Abs(unsquashedY) / s32) * 2f; 
                
                // Убираем отрицательную ширину на самых краях
                if (w < 0) w = 0; 
                
                // Рисуем горизонтальную линию
                sb.Draw(_tex, new Rectangle((int)Math.Round(c.X - w/2f), (int)Math.Round(c.Y + y), (int)Math.Round(w), 2), col);
            }
        }

        // --- ОСТАЛЬНЫЕ МЕТОДЫ (DrawAttackEffect, DrawNpc и UI) БЕЗ ИЗМЕНЕНИЙ ---
        private void DrawAttackEffect(SpriteBatch sb, PlayerModel p) {
            Color c = GetZoneColor(p.CurrentZone); Vector2 cPos = p.Position + new Vector2(25, 25); 
            if (p.CurrentZone == AnomalyType.Neutral) {
                if (p.ActiveSlot == 0) DrawLine(sb, cPos, cPos + p.FacingDir * 1000f, c, 4); 
                else if (p.ActiveSlot == 2) DrawLine(sb, cPos, cPos + p.FacingDir * 1000f, Color.LimeGreen, 6); 
            } else if (p.CurrentZone == AnomalyType.Red) {
                if (p.ActiveSlot == 1) DrawCircle(sb, cPos, 400f, c, 3); 
                else if (p.ActiveSlot == 2) DrawLine(sb, cPos, cPos + p.FacingDir * 2000f, c, 6); 
                else DrawCone(sb, cPos, p.FacingDir, 200f, 0.5f, c); 
            } else if (p.CurrentZone == AnomalyType.Blue) {
                if (p.ActiveSlot == 1) DrawCircle(sb, cPos, 250f, c, 4); 
                else if (p.ActiveSlot == 2) { DrawCircle(sb, cPos, 500f, c, 1); DrawCircle(sb, cPos, 800f, c, 1); }
                else DrawCone(sb, cPos, p.FacingDir, 100f, 0.5f, c); 
            }
        }
        private void DrawNpc(SpriteBatch sb, NpcModel npc) {
            sb.Draw(_tex, new Rectangle((int)npc.Position.X, (int)npc.Position.Y, 40, 40), Color.Yellow);
            sb.Draw(_tex, new Rectangle((int)npc.Position.X, (int)npc.Position.Y - 10, (int)(40 * (npc.Health/100f)), 5), Color.LimeGreen);
        }
        private void DrawEnemy(SpriteBatch sb, EnemyModel e) {
            Color eCol = e.Type == EnemyType.Shooter ? Color.DarkRed : Color.Purple;
            if (e.StunTimer > 0) eCol = Color.White; else if (e.SlowTimer > 0) eCol = Color.CornflowerBlue;
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y, 40, 40), eCol);
            if (e.Type == EnemyType.Shooter) DrawCircle(sb, new Vector2(e.Position.X + 20, e.Position.Y + 20), 30f, Color.Red, 2);
            sb.Draw(_tex, new Rectangle((int)e.Position.X, (int)e.Position.Y - 10, (int)(40 * (e.Health/e.MaxHealth)), 5), Color.Red);
            if (e.IsInfected) sb.Draw(_tex, new Rectangle((int)e.Position.X + 15, (int)e.Position.Y - 20, 10, 10), Color.LimeGreen);
        }
        public void DrawUI(SpriteBatch sb, PlayerModel p, SpriteFont font, int w, int h, int lvl) {
            sb.Draw(_tex, new Rectangle(20, 20, (int)(200 * (p.CurrentHealth/p.MaxHealth)), 20), Color.Red);
            int uiX = w - 100; int uiY = h - 120;
            sb.Draw(_tex, new Rectangle(uiX, uiY, 60, 60), GetZoneColor(p.CurrentZone));
            float currentCd = p.ActiveSlot == 0 ? p.Cd1 : (p.ActiveSlot == 1 ? p.Cd2 : p.Cd3);
            float maxCd = p.ActiveSlot == 0 ? p.MaxCd1 : (p.ActiveSlot == 1 ? p.MaxCd2 : p.MaxCd3);
            sb.Draw(_tex, new Rectangle(uiX, uiY, 60, (int)(60 * (maxCd > 0 ? currentCd / maxCd : 0))), Color.Black * 0.7f);
            sb.Draw(_tex, new Rectangle(uiX, uiY + 70, 60, 10), Color.DarkBlue);
            sb.Draw(_tex, new Rectangle(uiX, uiY + 70, (int)(60 * (p.CurrentMana/100f)), 10), Color.Cyan);
            if (font != null) {
                sb.DrawString(font, $"SLOT: {p.ActiveSlot + 1}", new Vector2(uiX, uiY - 30), Color.White);
                sb.DrawString(font, $"LEVEL: {lvl}", new Vector2(w / 2 - 40, 20), Color.White);
            }
        }
        public void DrawPauseMenu(SpriteBatch sb, SpriteFont f, int w, int h) {
            sb.Draw(_tex, new Rectangle(0, 0, w, h), Color.Black * 0.6f);
            if (f != null) {
                sb.DrawString(f, "=== PAUSED ===", new Vector2(w/2 - 60, h/2 - 50), Color.White);
                sb.DrawString(f, "Press ESC to Resume", new Vector2(w/2 - 80, h/2), Color.LightGray);
                sb.DrawString(f, "Press R to Restart", new Vector2(w/2 - 75, h/2 + 30), Color.LightGray);
            }
        }
        private Color GetZoneColor(AnomalyType t) => t == AnomalyType.Red ? Color.Red : (t == AnomalyType.Blue ? Color.Blue : Color.Green);
        private void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, Color c, int t=2) {
            Vector2 e = p2 - p1; float angle = (float)Math.Atan2(e.Y, e.X);
            sb.Draw(_tex, new Rectangle((int)p1.X, (int)p1.Y, (int)e.Length(), t), null, c, angle, new Vector2(0, 0.5f), 0, 0);
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
            DrawLine(sb, c, p1, col, 3); DrawLine(sb, c, p2, col, 3); DrawLine(sb, p1, c + d * r, col, 2); DrawLine(sb, c + d * r, p2, col, 2);
        }
    }
}
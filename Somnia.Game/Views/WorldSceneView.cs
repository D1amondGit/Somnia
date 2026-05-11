using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Models.Particles;
using Somnia.Game.Services.AI;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

public sealed class WorldSceneView
{
    private readonly SpritePrimitiveRenderer _prim;

    public WorldSceneView(GraphicsDevice device) => _prim = new SpritePrimitiveRenderer(device);

    public void Draw(
        SpriteBatch sb,
        Rectangle playArea,
        Texture2D? floorTexture,
        PlayerModel player,
        IReadOnlyList<EnemyModel> enemies,
        IReadOnlyList<AnomalyZone> zones,
        NpcModel npc,
        IReadOnlyList<HexagonModel> walls,
        Texture2D? wallTexture,
        IReadOnlyList<ResourceDropModel> drops,
        IReadOnlyList<GateModel> gates,
        IReadOnlyList<FloatingText> floatingTexts,
        SpriteFont? font,
        IReadOnlyList<ProjectileModel> enemyProj,
        IReadOnlyList<PlayerProjectileModel> playerProj,
        IReadOnlyList<FloorSplatter>? floorSplatters = null,
        IReadOnlyList<WallSparkle>? wallSparkles = null,
        double timeSec = 0.0)
    {
        DrawPlayFloor(sb, playArea, floorTexture, _prim.PixelTexture);

        DrawZones(sb, zones);
        // Сначала «следы» на полу (тёмные scorch, тусклые пятна) — под стенами и сущностями.
        DrawFloorSplatters(sb, floorSplatters, onlyScorch: true);
        DrawWalls(sb, walls, wallTexture);
        DrawWallSparkles(sb, wallSparkles);
        DrawGates(sb, gates);
        DrawOutskirts(sb, playArea);
        DrawDrops(sb, drops);
        DrawSniperTelegraphs(sb, enemies, player, timeSec);
        DrawBossTelegraphs(sb, enemies, timeSec);
        DrawEnemyProjectiles(sb, enemyProj);
        DrawPlayerProjectiles(sb, playerProj);
        DrawAttackPreview(sb, player);

        DrawNpc(sb, npc);
        DrawEnemies(sb, enemies);
        DrawMuzzleFlashes(sb, enemies);
        DrawPlayer(sb, player);

        // Яркие брызги — сверху всего, чтобы было видно «момент попадания».
        DrawFloorSplatters(sb, floorSplatters, onlyScorch: false);

        if (player.IsShieldActive)
        {
            var pulse = 0.5f + 0.5f * (float)System.Math.Sin(player.ShieldTimer * 12f);
            _prim.DrawCircleOutline(sb, player.Position, player.ShieldRadius,
                new Color(120, 230, 130) * (0.4f + pulse * 0.5f), 3);
            _prim.DrawCircleOutline(sb, player.Position, player.ShieldRadius * 0.7f,
                new Color(160, 240, 160) * (0.3f + pulse * 0.4f), 2);
        }

        DrawFloatingTexts(sb, font, floatingTexts);
    }

    /// <summary>
    /// Пол без процедурной генерации: тайлится <paramref name="floorTexture"/> если задана
    /// (положи свою в Content/floor.png), иначе — ровная заливка под зоны и сущности.
    /// </summary>
    private static void DrawPlayFloor(SpriteBatch sb, Rectangle playArea, Texture2D? floorTexture,
        Texture2D pixelFallback)
    {
        if (floorTexture is { Width: > 0, Height: > 0 })
        {
            for (var y = playArea.Top; y < playArea.Bottom; y += floorTexture.Height)
            for (var x = playArea.Left; x < playArea.Right; x += floorTexture.Width)
            {
                var w = System.Math.Min(floorTexture.Width, playArea.Right - x);
                var h = System.Math.Min(floorTexture.Height, playArea.Bottom - y);
                sb.Draw(floorTexture, new Rectangle(x, y, w, h), new Rectangle(0, 0, w, h), Color.White);
            }
        }
        else
            sb.Draw(pixelFallback, playArea, new Color(12, 14, 20));
    }

    private void DrawFloorSplatters(SpriteBatch sb, IReadOnlyList<FloorSplatter>? splatters, bool onlyScorch)
    {
        if (splatters == null) return;
        var tex = _prim.PixelTexture;
        foreach (var s in splatters)
        {
            if (onlyScorch && !s.IsScorch) continue;
            if (!onlyScorch && s.IsScorch) continue;

            var alpha = s.Alpha;

            if (s.IsScorch)
            {
                // Гарь — широкие тёмные пятна на полу с лёгким squash.
                var size = (int)(s.Radius * 2.4f);
                if (size < 2) continue;
                var h = (int)(size * IsometricView.Squash);
                if (h < 1) h = 1;
                sb.Draw(tex,
                    new Rectangle((int)s.Position.X - size / 2, (int)s.Position.Y - h / 2, size, h),
                    s.Color * (alpha * 0.55f));
            }
            else
            {
                // Брызги — мелкие яркие квадраты, почти без squash, чтобы было заметно сверху.
                var size = (int)MathHelper.Max(3f, s.Radius * 1.6f);
                sb.Draw(tex,
                    new Rectangle((int)s.Position.X - size / 2, (int)s.Position.Y - size / 2, size, size),
                    s.Color * alpha);
                // Внутренний «hot core» — белёсая точка
                var coreSize = MathHelper.Max(1, size / 3);
                sb.Draw(tex,
                    new Rectangle((int)s.Position.X - coreSize / 2, (int)s.Position.Y - coreSize / 2,
                        coreSize, coreSize),
                    Color.Lerp(s.Color, Color.White, 0.5f) * alpha);
            }
        }
    }

    private void DrawWallSparkles(SpriteBatch sb, IReadOnlyList<WallSparkle>? sparkles)
    {
        if (sparkles == null) return;
        var tex = _prim.PixelTexture;
        foreach (var s in sparkles)
        {
            var size = (int)(s.Size * 2);
            if (size < 1) size = 1;
            sb.Draw(tex, new Rectangle((int)s.Position.X - size / 2, (int)s.Position.Y - size / 2, size, size),
                s.Color * s.Alpha);
        }
    }

    private void DrawZones(SpriteBatch sb, IReadOnlyList<AnomalyZone> zones)
    {
        foreach (var z in zones)
        {
            var col = SpritePrimitiveRenderer.ZoneFlashColor(z.Type);
            _prim.FillPoly(sb, z.Outline, col * 0.22f);
            DrawClosedOutline(sb, z.Outline, col * 0.55f);
        }
    }

    private void DrawWalls(SpriteBatch sb, IReadOnlyList<HexagonModel> walls, Texture2D? wallTexture)
    {
        foreach (var w in walls)
            SpritePrimitiveRenderer.DrawHexWalls(sb, _prim, w, wallTexture);

        foreach (var w in walls)
        {
            var top = w.GetTopVertices();

            if (w.IsDestructible)
            {
                // Разрушаемое укрытие: цвет верхушки зависит от оставшегося HP.
                var frac = MathHelper.Clamp(w.DestructibleHealth / w.MaxDestructibleHealth, 0f, 1f);
                var topColor = Color.Lerp(new Color(60, 18, 20), new Color(40, 30, 25), frac);
                _prim.FillPoly(sb, top, topColor);

                // Edge — тёплый/тревожный жёлто-красный, чтобы укрытие читалось как «бьющееся».
                var edge = Color.Lerp(new Color(255, 80, 50), new Color(255, 200, 120), frac);
                for (var i = 0; i < top.Count; i++)
                    _prim.DrawLine(sb, top[i], top[(i + 1) % top.Count], edge, 2);

                // Трещины при низком HP — чёрные полоски через центр.
                if (frac < 0.6f)
                {
                    var cracks = frac < 0.3f ? 4 : 2;
                    for (var i = 0; i < cracks; i++)
                    {
                        var a = i * MathHelper.TwoPi / cracks + frac * 3f;
                        var dir = new Vector2((float)System.Math.Cos(a), (float)System.Math.Sin(a) * IsometricView.Squash);
                        _prim.DrawLine(sb, w.Center - dir * w.Radius * 0.4f,
                            w.Center + dir * w.Radius * 0.6f,
                            new Color(0, 0, 0, 220), 2);
                    }
                }
            }
            else
            {
                // Обычная стена: чёрная верхушка + холодная кромка.
                _prim.FillPoly(sb, top, Color.Black);
                for (var i = 0; i < top.Count; i++)
                    _prim.DrawLine(sb, top[i], top[(i + 1) % top.Count], new Color(150, 165, 195), 2);
            }
        }
    }

    private void DrawGates(SpriteBatch sb, IReadOnlyList<GateModel> gates)
    {
        foreach (var g in gates)
        {
            var h = new HexagonModel(g.Position, 80f, 40f, IsometricView.Squash, IsometricView.Tilt);
            _prim.FillPoly(sb, h.GetTopVertices(), g.IsOpen ? Color.LimeGreen : Color.Red * 0.5f);
        }
    }

    private void DrawDrops(SpriteBatch sb, IReadOnlyList<ResourceDropModel> drops)
    {
        foreach (var d in drops)
            _prim.DrawCircleOutline(sb, d.Position, 6f, d.Type == DropType.Health ? Color.Red : Color.Cyan, 5);
    }

    private void DrawSniperTelegraphs(SpriteBatch sb, IReadOnlyList<EnemyModel> enemies, PlayerModel player,
        double timeSec)
    {
        foreach (var e in enemies)
        {
            if (e.IsDead || !e.IsTelegraphing) continue;
            if (e.Type != EnemyType.Sniper) continue;

            // Прицел рисуется красивым «лазером»: свечение + пунктир + крестик в цели.
            var color = new Color(255, 70, 70);
            IsoEntityRenderer.DrawLaserSight(sb, _prim, e.Position, player.Position, color, timeSec);
        }
    }

    private void DrawBossTelegraphs(SpriteBatch sb, IReadOnlyList<EnemyModel> enemies, double timeSec)
    {
        foreach (var e in enemies)
        {
            if (e.IsDead || e.Type != EnemyType.Boss) continue;

            if (e.BossPhase == BossAttackPhase.SlamTelegraph)
            {
                var progress = 1f - MathHelper.Clamp(
                    e.BossPhaseTimer / BossController.SlamTelegraphTime, 0f, 1f);
                IsoEntityRenderer.DrawAoeTelegraph(sb, _prim, e.BossActionCenter,
                    e.BossActionRadius, new Color(255, 90, 90), progress);
            }
            else if (e.BossPhase == BossAttackPhase.VolleyTelegraph)
            {
                // Веер из стрелок к будущей точке стрельбы.
                var to = e.BossActionCenter - e.Position;
                if (to != Vector2.Zero)
                {
                    to.Normalize();
                    var color = new Color(255, 180, 80) * (0.6f + 0.4f * (float)System.Math.Sin(timeSec * 18));
                    for (var i = -3; i <= 3; i++)
                    {
                        var a = i * 0.16f;
                        var dir = new Vector2(
                            to.X * (float)System.Math.Cos(a) - to.Y * (float)System.Math.Sin(a),
                            to.X * (float)System.Math.Sin(a) + to.Y * (float)System.Math.Cos(a));
                        _prim.DrawLine(sb, e.Position, e.Position + dir * 600f, color, 2);
                    }
                }
            }
        }
    }

    private void DrawMuzzleFlashes(SpriteBatch sb, IReadOnlyList<EnemyModel> enemies)
    {
        foreach (var e in enemies)
        {
            if (e.IsDead || e.MuzzleFlashTimer <= 0f) continue;
            var origin = e.Position - new Vector2(0, e.Archetype.BodyHeight * 0.6f);
            var color = e.Type == EnemyType.Sniper
                ? new Color(255, 120, 120)
                : e.Type == EnemyType.Boss
                    ? new Color(255, 200, 90)
                    : new Color(255, 200, 90);
            var strength = MathHelper.Clamp(e.MuzzleFlashTimer / 0.14f, 0f, 1f);
            IsoEntityRenderer.DrawMuzzleFlash(sb, _prim, origin, e.MuzzleFlashDir, strength, color);
        }
    }

    private void DrawEnemyProjectiles(SpriteBatch sb, IReadOnlyList<ProjectileModel> enemyProj)
    {
        var core = new Color(255, 160, 80);
        foreach (var ep in enemyProj)
        {
            _prim.DrawLine(sb, ep.Position - ep.Velocity * 0.03f, ep.Position + ep.Velocity * 0.015f, core * 0.35f, 5);
            _prim.DrawCircleOutline(sb, ep.Position, ep.Radius, core, 3);
            _prim.DrawCircleOutline(sb, ep.Position, ep.Radius + 3f, core * 0.4f, 2);
        }
    }

    private void DrawPlayerProjectiles(SpriteBatch sb, IReadOnlyList<PlayerProjectileModel> playerProj)
    {
        foreach (var pp in playerProj)
        {
            var c = pp.Kind switch
            {
                PlayerProjectileKind.Rocket => new Color(255, 130, 70),
                PlayerProjectileKind.Pellet => new Color(255, 220, 140),
                PlayerProjectileKind.Grenade => new Color(140, 240, 140),
                _ => new Color(180, 230, 255)
            };

            if (pp.Kind == PlayerProjectileKind.Grenade)
            {
                _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 7f, c, 5);
                _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 12f, c * 0.5f, 2);
                _prim.DrawCircleOutline(sb, pp.Position, pp.ExplosionRadius, c * 0.18f, 2);
            }
            else if (pp.Kind == PlayerProjectileKind.Pellet)
            {
                // Дробь: маленький светящийся шар + хвост-блик
                _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 1.5f, c, 3);
                _prim.DrawLine(sb, pp.Position - pp.Velocity * 0.012f,
                    pp.Position + pp.Velocity * 0.02f, c * 0.85f, 2);
            }
            else
            {
                // Тип луча/болта: яркое ядро + длинный плазменный хвост
                _prim.DrawLine(sb, pp.Position - pp.Velocity * 0.05f,
                    pp.Position + pp.Velocity * 0.025f, c * 0.35f, 6);
                _prim.DrawLine(sb, pp.Position - pp.Velocity * 0.025f,
                    pp.Position + pp.Velocity * 0.025f, c, 3);
                _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 1f, c, 3);
                _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 4f, c * 0.3f, 2);
            }
        }
    }

    private void DrawFloatingTexts(SpriteBatch sb, SpriteFont? font, IReadOnlyList<FloatingText> texts)
    {
        if (font == null) return;
        // Чёткий обычный шрифт без shake — только плавное всплывание/затухание
        // (этим занимается FloatingText.Update, тут только отрисовка).
        foreach (var t in texts)
        {
            var pos = t.Position + new Vector2(-20, -50);
            var alpha = MathHelper.Clamp(t.Timer, 0f, 1f);
            sb.DrawString(font, t.Text, pos + new Vector2(1, 1), Color.Black * (0.7f * alpha));
            sb.DrawString(font, t.Text, pos, t.Color * alpha);
        }
    }

    private void DrawClosedOutline(SpriteBatch sb, IReadOnlyList<Vector2> verts, Color color)
    {
        for (var i = 0; i < verts.Count; i++)
            _prim.DrawLine(sb, verts[i], verts[(i + 1) % verts.Count], color, thickness: 2);
    }

    private void DrawOutskirts(SpriteBatch sb, Rectangle p)
    {
        var t = _prim.PixelTexture;
        sb.Draw(t, new Rectangle(-2000, -2000, 8000, p.Y + 2000), Color.Black);
        sb.Draw(t, new Rectangle(-2000, p.Bottom, 8000, 2000), Color.Black);
        sb.Draw(t, new Rectangle(-2000, p.Y, p.X + 2000, p.Height), Color.Black);
        sb.Draw(t, new Rectangle(p.Right, p.Y, 2000, p.Height), Color.Black);
    }

    private void DrawPlayer(SpriteBatch sb, PlayerModel p)
    {
        var body = PlayerPalette.GetBodyColor(p);
        var accent = PlayerPalette.GetAccentColor(p);
        IsoEntityRenderer.DrawCharacter(sb, _prim, p.Position, baseRadius: 18f, height: 40f, body, accent);
        IsoEntityRenderer.DrawHealthBar(sb, _prim, p.Position, characterHeight: 40f, widthPx: 48f,
            fraction: p.CurrentHealth / p.MaxHealth, barColor: new Color(255, 70, 70));
    }

    private void DrawNpc(SpriteBatch sb, NpcModel npc)
    {
        if (npc.IsPickedUp || npc.IsDead) return;
        var body = npc.IsInjured ? new Color(220, 180, 80) : new Color(240, 220, 90);
        var accent = new Color(255, 240, 160);
        IsoEntityRenderer.DrawCharacter(sb, _prim, npc.Position, baseRadius: 17f, height: 34f, body, accent);
        IsoEntityRenderer.DrawHealthBar(sb, _prim, npc.Position, characterHeight: 34f, widthPx: 44f,
            fraction: npc.Health / npc.MaxHealth, barColor: new Color(120, 230, 130));
    }

    private void DrawEnemies(SpriteBatch sb, IReadOnlyList<EnemyModel> enemies)
    {
        foreach (var e in enemies)
        {
            if (e.IsDead) continue;
            var a = e.Archetype;
            var body = ResolveEnemyColor(e);
            IsoEntityRenderer.DrawCharacter(sb, _prim, e.Position, a.BodyRadius, a.BodyHeight, body, a.AccentColor);
            IsoEntityRenderer.DrawHealthBar(sb, _prim, e.Position, a.BodyHeight, a.BodyRadius * 2.2f,
                e.Health / e.MaxHealth, new Color(220, 60, 60));
        }
    }

    private static Color ResolveEnemyColor(EnemyModel e)
    {
        if (e.DamageFlash > 0) return Color.White;
        if (e.StunTimer > 0) return Color.LightGray;
        if (e.SlowTimer > 0) return Color.CornflowerBlue;
        if (e.IsTelegraphing) return Color.Lerp(e.Archetype.BodyColor, Color.White, 0.5f);
        return e.Archetype.BodyColor;
    }

    private void DrawAttackPreview(SpriteBatch sb, PlayerModel p)
    {
        if (!p.IsAttacking) return;
        var cPos = p.Position;
        switch (p.CurrentZone)
        {
            case AnomalyType.Neutral when p.ActiveSlot == 0:
                // Автомат — короткая полоса трассеров
                _prim.DrawLine(sb, cPos, cPos + p.FacingDir * 720f, Color.LightYellow, 4);
                break;
            case AnomalyType.Red when p.ActiveSlot == 0:
                // Дробовик — конус
                _prim.DrawCone(sb, cPos, p.FacingDir, 320f, 0.6f, Color.Red);
                break;
            case AnomalyType.Blue when p.ActiveSlot == 0:
                // Снайперка — луч через всю арену
                _prim.DrawLine(sb, cPos, cPos + p.FacingDir * 2400f, Color.DeepSkyBlue, 5);
                break;
            case AnomalyType.Green when p.ActiveSlot == 0:
                // Граната — превью точки разрыва
                _prim.DrawCircleOutline(sb, cPos + p.FacingDir * 320f, 260f, Color.LimeGreen, 3);
                break;
            case AnomalyType.Red when p.ActiveSlot == 2:
                _prim.DrawCircleOutline(sb, cPos, 300f, Color.Red, 10);
                break;
            case AnomalyType.Blue when p.ActiveSlot == 1:
                _prim.DrawCircleOutline(sb, cPos, 250f, Color.Blue, 5);
                break;
        }
    }
}

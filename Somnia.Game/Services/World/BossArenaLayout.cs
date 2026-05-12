using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>
/// Спец-лэйаут финальной арены: пустой центр под босса, разрушаемые укрытия
/// по кругу — за ними нужно прятать NPC от AoE-slam.
/// </summary>
public static class BossArenaLayout
{
    public const float CoverRadius = 95f;
    public const float CoverHeight = 52f;
    public const float CoverHealth = 160f;
    public const int CoverCount = 8;

    /// <summary>Как часто оркестратор сдвигает типы зон (секунды).</summary>
    public const float AnomalyShiftIntervalSeconds = 5.4f;

    public static ArenaLayout Build(Rectangle playArea, int seed)
    {
        var rnd = new Random(seed);
        var walls = new List<HexagonModel>();

        // Граничные стены — переиспользуем из общего генератора (через прямой вызов кода).
        BuildBoundary(playArea, walls);

        // Кольцо разрушаемых укрытий вокруг центра арены.
        var center = new Vector2(playArea.Center.X + 60f, playArea.Center.Y);
        var ringRadius = MathHelper.Min(playArea.Width, playArea.Height) * 0.22f;

        for (var i = 0; i < CoverCount; i++)
        {
            var angle = MathHelper.TwoPi * i / CoverCount + (float)rnd.NextDouble() * 0.15f;
            var pos = center + new Vector2(
                MathF.Cos(angle) * ringRadius,
                MathF.Sin(angle) * ringRadius * IsometricView.Squash);

            var wall = new HexagonModel(pos, CoverRadius, CoverHeight,
                IsometricView.Squash, IsometricView.Tilt, rotationRadians: angle)
            {
                MaxDestructibleHealth = CoverHealth,
                DestructibleHealth = CoverHealth
            };
            walls.Add(wall);
        }

        // Пара длинных «направляющих» — даёт направление, куда тащить NPC.
        for (var x = playArea.Left + 220f; x < playArea.Right - 220f; x += 240f)
        {
            // Узкий «коридор» из мелких разрушаемых блоков по верху и низу.
            walls.Add(MakeCover(new Vector2(x, playArea.Top + playArea.Height * 0.18f), seed));
            walls.Add(MakeCover(new Vector2(x, playArea.Top + playArea.Height * 0.82f), seed));
        }

        var zones = BuildShiftingAnomalyZones(playArea, rnd);
        return new ArenaLayout(zones, walls, seed);
    }

    /// <summary>
    /// Цветные зоны по полу: между спавном слева и боссом справа. Типы меняются в рантайме
    /// (см. оркестратор), формы остаются.
    /// </summary>
    public static AnomalyZone[] BuildShiftingAnomalyZones(Rectangle playArea, Random rnd)
    {
        var combatTypes = new[] { AnomalyType.Red, AnomalyType.Blue, AnomalyType.Green };
        var spawnSafe = new Vector2(playArea.Left + 250f, playArea.Top + playArea.Height * 0.5f);
        var bossBias = new Vector2(playArea.Left + playArea.Width * 0.72f, playArea.Top + playArea.Height * 0.5f);
        var gateSafe = new Vector2(playArea.Right - 200f, playArea.Top + playArea.Height * 0.5f);

        var slots = new Vector2[]
        {
            new(playArea.Left + playArea.Width * 0.38f, playArea.Top + playArea.Height * 0.28f),
            new(playArea.Left + playArea.Width * 0.48f, playArea.Top + playArea.Height * 0.42f),
            new(playArea.Left + playArea.Width * 0.42f, playArea.Top + playArea.Height * 0.62f),
            new(playArea.Left + playArea.Width * 0.55f, playArea.Top + playArea.Height * 0.72f),
            new(playArea.Left + playArea.Width * 0.58f, playArea.Top + playArea.Height * 0.35f),
            new(playArea.Left + playArea.Width * 0.62f, playArea.Top + playArea.Height * 0.55f),
            new(playArea.Left + playArea.Width * 0.33f, playArea.Top + playArea.Height * 0.48f),
            new(playArea.Left + playArea.Width * 0.50f, playArea.Top + playArea.Height * 0.22f)
        };

        var list = new List<AnomalyZone>(slots.Length);
        foreach (var baseCenter in slots)
        {
            var jitter = new Vector2(
                ((float)rnd.NextDouble() - 0.5f) * 70f,
                ((float)rnd.NextDouble() - 0.5f) * 50f);
            var c = baseCenter + jitter;

            if (Vector2.Distance(c, spawnSafe) < 175f) continue;
            if (Vector2.Distance(c, gateSafe) < 150f) continue;
            if (Vector2.Distance(c, bossBias) < 120f) continue;

            var radius = rnd.Next(105, 152);
            var outline = ZoneShapeFactory.BuildOrganicOutline(c, radius, rnd);
            var type = combatTypes[rnd.Next(combatTypes.Length)];
            list.Add(new AnomalyZone(c, radius, type, outline));
        }

        return list.ToArray();
    }

    /// <summary>Сдвиг по кругу Red→Blue→Green для каждой зоны (вызывается из игрового цикла).</summary>
    public static void CycleAnomalyZoneTypes(IList<AnomalyZone> zones)
    {
        for (var i = 0; i < zones.Count; i++)
        {
            var z = zones[i];
            z.Type = z.Type switch
            {
                AnomalyType.Red => AnomalyType.Blue,
                AnomalyType.Blue => AnomalyType.Green,
                AnomalyType.Green => AnomalyType.Red,
                _ => AnomalyType.Red
            };
        }
    }

    private static HexagonModel MakeCover(Vector2 pos, int seed)
    {
        var rotation = (seed * 0.0001f + pos.X * 0.001f) % MathHelper.PiOver2;
        return new HexagonModel(pos, CoverRadius * 0.85f, CoverHeight,
            IsometricView.Squash, IsometricView.Tilt, rotation)
        {
            MaxDestructibleHealth = CoverHealth * 0.75f,
            DestructibleHealth = CoverHealth * 0.75f
        };
    }

    private static void BuildBoundary(Rectangle playArea, List<HexagonModel> walls)
    {
        var w = playArea.Width;
        var h = playArea.Height;
        var step = ArenaHexGrid.HorizontalSpacing * 0.98f;
        const float boundaryRadius = ArenaLayoutGenerator.BoundaryRadius;
        const float boundaryHeight = ArenaLayoutGenerator.BoundaryWallHeight;

        for (var x = -step; x <= w + step; x += step)
        {
            walls.Add(new HexagonModel(new Vector2(x, -110f), boundaryRadius, boundaryHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
            walls.Add(new HexagonModel(new Vector2(x, h + 110f), boundaryRadius, boundaryHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
        }
        for (var y = -step; y <= h + step; y += step)
        {
            walls.Add(new HexagonModel(new Vector2(-110f, y), boundaryRadius, boundaryHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
            walls.Add(new HexagonModel(new Vector2(w + 110f, y), boundaryRadius, boundaryHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
        }
    }
}

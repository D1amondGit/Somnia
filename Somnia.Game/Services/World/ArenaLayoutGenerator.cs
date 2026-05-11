using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>
/// Генерация арены:
///  • Зоны раскидываются стратифицированно (грид-бакеты + jitter), а не «по гекс-сетке», —
///    благодаря этому они равномерно расходятся по всей арене.
///  • Стены/укрытия — фиксированного размера, высоты и угла (стиль Quickerflack).
/// </summary>
public sealed class ArenaLayoutGenerator
{
    public const float ObstacleRadius = 110f;
    /// <summary>Высота стен-препятствий. Пониже — стены меньше закрывают обзор и поворот гекса
    /// читается лучше. Не путать с <see cref="ArenaHexGrid"/>, у которого свои размеры пола.</summary>
    public const float ObstacleWallHeight = 48f;
    public const float BoundaryRadius = 215f;
    public const float BoundaryWallHeight = 70f;

    public ArenaLayout Generate(Rectangle playArea, int seed, int anomalyTargetCount = 12)
    {
        var rnd = new Random(seed);
        var anomalyTypes = new[] { AnomalyType.Red, AnomalyType.Blue, AnomalyType.Green };

        var zones = ScatterZonesStratified(playArea, rnd, anomalyTypes,
            Math.Max(anomalyTargetCount, 15));

        var walls = new List<HexagonModel>();
        BuildBoundaryWalls(playArea, walls);
        BuildBarrierLines(playArea, rnd, zones, walls);
        ScatterFixedSizeObstacles(playArea, rnd, zones, walls);

        return new ArenaLayout(zones, walls, seed);
    }

    public List<AnomalyZone> RegenerateZonesOnly(Rectangle playArea, int seed, int anomalyTargetCount = 12) =>
        Generate(playArea, seed, anomalyTargetCount).Zones.ToList();

    /// <summary>
    /// Делит арену на сетку cols×rows и кладёт ровно одну зону в каждый бакет
    /// с лёгким случайным смещением. Это убирает «комкование» вокруг центра.
    /// </summary>
    private static List<AnomalyZone> ScatterZonesStratified(
        Rectangle playArea,
        Random rnd,
        AnomalyType[] types,
        int targetCount)
    {
        var (cols, rows) = ChooseBucketGrid(playArea, targetCount);
        var marginX = playArea.Width * 0.06f;
        var marginY = playArea.Height * 0.08f;

        var innerW = playArea.Width - marginX * 2f;
        var innerH = playArea.Height - marginY * 2f;
        var cellW = innerW / cols;
        var cellH = innerH / rows;

        var spawnZone = new Vector2(playArea.Left + 250f, playArea.Top + playArea.Height / 2f);
        var gateZone = new Vector2(playArea.Right - 200f, playArea.Top + playArea.Height / 2f);

        var buckets = new List<(int Col, int Row)>(cols * rows);
        for (var c = 0; c < cols; c++)
        for (var r = 0; r < rows; r++)
            buckets.Add((c, r));

        ShuffleInPlace(buckets, rnd);

        var zones = new List<AnomalyZone>();
        foreach (var (c, r) in buckets)
        {
            if (zones.Count >= targetCount) break;

            var jitterX = ((float)rnd.NextDouble() - 0.5f) * cellW * 0.68f;
            var jitterY = ((float)rnd.NextDouble() - 0.5f) * cellH * 0.68f;

            var cx = playArea.Left + marginX + cellW * (c + 0.5f) + jitterX;
            var cy = playArea.Top + marginY + cellH * (r + 0.5f) + jitterY;
            var center = new Vector2(cx, cy);

            if (Vector2.Distance(center, spawnZone) < 180f) continue;
            if (Vector2.Distance(center, gateZone) < 180f) continue;

            var zr = rnd.Next(118, 178);
            if (zones.Any(z => Vector2.Distance(center, z.Center) < (z.Radius + zr) * 0.48f)) continue;

            var outline = ZoneShapeFactory.BuildOrganicOutline(center, zr, rnd);
            zones.Add(new AnomalyZone(center, zr, types[rnd.Next(types.Length)], outline));
        }

        return zones;
    }

    private static (int Cols, int Rows) ChooseBucketGrid(Rectangle playArea, int targetCount)
    {
        var aspect = playArea.Width / (float)Math.Max(playArea.Height, 1);
        var rows = (int)Math.Round(Math.Sqrt(targetCount / aspect));
        rows = Math.Clamp(rows, 2, 6);
        var cols = (int)Math.Ceiling(targetCount / (float)rows);
        cols = Math.Clamp(cols, 2, 8);
        return (cols, rows);
    }

    private static void BuildBoundaryWalls(Rectangle playArea, List<HexagonModel> walls)
    {
        var w = playArea.Width;
        var h = playArea.Height;
        var step = ArenaHexGrid.HorizontalSpacing * 0.98f;

        for (var x = -step; x <= w + step; x += step)
        {
            walls.Add(new HexagonModel(new Vector2(x, -110f), BoundaryRadius, BoundaryWallHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
            walls.Add(new HexagonModel(new Vector2(x, h + 110f), BoundaryRadius, BoundaryWallHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
        }

        for (var y = -step; y <= h + step; y += step)
        {
            walls.Add(new HexagonModel(new Vector2(-110f, y), BoundaryRadius, BoundaryWallHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
            walls.Add(new HexagonModel(new Vector2(w + 110f, y), BoundaryRadius, BoundaryWallHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt));
        }
    }

    /// <summary>
    /// Препятствия фиксированного размера, разнесённые по бакетам внутренней арены.
    /// </summary>
    private static void ScatterFixedSizeObstacles(Rectangle playArea, Random rnd,
        List<AnomalyZone> zones, List<HexagonModel> walls)
    {
        var spawn = new Vector2(playArea.Left + 250f, playArea.Top + playArea.Height / 2f);
        var gate = new Vector2(playArea.Right - 200f, playArea.Top + playArea.Height / 2f);

        const float keepFromZone = 60f;
        const float minObstacleDistance = ObstacleRadius * 2.4f;
        const float keepFromSpawn = 320f;
        const float keepFromGate = 280f;

        var (cols, rows) = (5, 3);
        var marginX = playArea.Width * 0.1f;
        var marginY = playArea.Height * 0.14f;
        var innerW = playArea.Width - marginX * 2f;
        var innerH = playArea.Height - marginY * 2f;
        var cellW = innerW / cols;
        var cellH = innerH / rows;

        var buckets = new List<(int Col, int Row)>(cols * rows);
        for (var c = 0; c < cols; c++)
        for (var r = 0; r < rows; r++)
            buckets.Add((c, r));

        ShuffleInPlace(buckets, rnd);

        var target = rnd.Next(6, 9);
        var placed = 0;

        foreach (var (c, r) in buckets)
        {
            if (placed >= target) break;

            var jitterX = ((float)rnd.NextDouble() - 0.5f) * cellW * 0.4f;
            var jitterY = ((float)rnd.NextDouble() - 0.5f) * cellH * 0.4f;

            var cx = playArea.Left + marginX + cellW * (c + 0.5f) + jitterX;
            var cy = playArea.Top + marginY + cellH * (r + 0.5f) + jitterY;
            var center = new Vector2(cx, cy);

            if (Vector2.Distance(center, spawn) < keepFromSpawn) continue;
            if (Vector2.Distance(center, gate) < keepFromGate) continue;
            if (zones.Any(z => Vector2.Distance(center, z.Center) < z.Radius + keepFromZone)) continue;
            if (walls.Any(w => Vector2.Distance(center, w.Center) < minObstacleDistance &&
                               w.Radius <= ObstacleRadius + 1f)) continue;

            // Каждая стена крутится на один из «дискретных» углов, чтобы поворот был
            // визуально читаемым (не путался с 6-fold симметрией хекса).
            var rotation = ObstacleRotations[rnd.Next(ObstacleRotations.Length)];
            walls.Add(new HexagonModel(center, ObstacleRadius, ObstacleWallHeight,
                ArenaHexGrid.Squash, ArenaHexGrid.Tilt, rotation));
            placed++;
        }
    }

    private static readonly float[] ObstacleRotations =
    {
        0f, MathHelper.Pi / 12f, MathHelper.Pi / 8f, MathHelper.Pi / 6f,
        -MathHelper.Pi / 12f, -MathHelper.Pi / 8f, -MathHelper.Pi / 6f,
    };

    /// <summary>
    /// Две вертикальные «стенки-барьера» с проходами в случайных местах.
    /// Создают лабиринтоподобную геометрию: NPC выгодно нести в обход,
    /// игрок может прятаться за барьером от снайперов.
    /// </summary>
    private static void BuildBarrierLines(Rectangle playArea, Random rnd,
        List<AnomalyZone> zones, List<HexagonModel> walls)
    {
        float[] xPositions =
        {
            playArea.Left + playArea.Width * 0.36f,
            playArea.Left + playArea.Width * 0.66f
        };

        const int hexCount = 6;
        var stepY = playArea.Height / (float)(hexCount + 1);

        foreach (var x in xPositions)
        {
            var gap = rnd.Next(hexCount);
            for (var i = 0; i < hexCount; i++)
            {
                if (i == gap) continue;
                if (i == (gap + 1) % hexCount) continue;

                var pos = new Vector2(x, playArea.Top + stepY * (i + 1));
                if (zones.Any(z => z.ContainsPoint(pos))) continue;

                var rotation = ObstacleRotations[rnd.Next(ObstacleRotations.Length)];
                walls.Add(new HexagonModel(pos, ObstacleRadius, ObstacleWallHeight,
                    ArenaHexGrid.Squash, ArenaHexGrid.Tilt, rotation));
            }
        }
    }

    private static void ShuffleInPlace<T>(IList<T> list, Random rnd)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

public sealed record ArenaLayout(IReadOnlyList<AnomalyZone> Zones, IReadOnlyList<HexagonModel> Walls, int Seed);

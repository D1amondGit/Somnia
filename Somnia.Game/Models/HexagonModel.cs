using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class HexagonModel
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }
    public float WallHeight { get; set; }
    public float Squash { get; set; }
    public float Tilt { get; set; }

    /// <summary>Локальный поворот гекса вокруг своего центра (в радианах).
    /// Не влияет на изометрию мира — крутится сам гекс, не камера.</summary>
    public float RotationRadians { get; set; }

    /// <summary>Если &gt; 0 — стена разрушаемая. Урон вычитается из <see cref="DestructibleHealth"/>.</summary>
    public float MaxDestructibleHealth { get; set; }

    /// <summary>Текущий HP разрушаемой стены. 0 = разрушена (удаляется из мира).</summary>
    public float DestructibleHealth { get; set; }

    public bool IsDestructible => MaxDestructibleHealth > 0f;
    public bool IsBroken => IsDestructible && DestructibleHealth <= 0f;

    public HexagonModel(
        Vector2 center,
        float radius,
        float wallHeight = 50f,
        float squash = 0.75f,
        float tilt = 0.1f,
        float rotationRadians = 0f)
    {
        Center = center;
        Radius = radius;
        WallHeight = wallHeight;
        Squash = squash;
        Tilt = tilt;
        RotationRadians = rotationRadians;
    }

    public List<Vector2> GetTopVertices() => GetVerticesAt(WallHeight);

    public List<Vector2> GetBaseVertices() => GetVerticesAt(0);

    private List<Vector2> GetVerticesAt(float z)
    {
        var v = new List<Vector2>();
        var cosR = (float)Math.Cos(RotationRadians);
        var sinR = (float)Math.Sin(RotationRadians);

        for (var i = 0; i < 6; i++)
        {
            var a = MathHelper.ToRadians(i * 60);
            var lx = (float)Math.Cos(a) * Radius;
            var ly = (float)Math.Sin(a) * Radius;

            // 1. Локальный поворот в плоскости гекса (как будто крутят сам гекс).
            var rx = lx * cosR - ly * sinR;
            var ry = lx * sinR + ly * cosR;

            // 2. Изометрическая проекция (сжатие + наклон от Tilt).
            var y = ry * Squash - rx * Tilt;
            v.Add(Center + new Vector2(rx, y - z));
        }

        return v;
    }
}

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

    public HexagonModel(
        Vector2 center,
        float radius,
        float wallHeight = 50f,
        float squash = 0.75f,
        float tilt = 0.1f)
    {
        Center = center;
        Radius = radius;
        WallHeight = wallHeight;
        Squash = squash;
        Tilt = tilt;
    }

    public List<Vector2> GetTopVertices() => GetVerticesAt(WallHeight);

    public List<Vector2> GetBaseVertices() => GetVerticesAt(0);

    private List<Vector2> GetVerticesAt(float z)
    {
        var v = new List<Vector2>();
        for (var i = 0; i < 6; i++)
        {
            var a = MathHelper.ToRadians(i * 60);
            var x = (float)Math.Cos(a) * Radius;
            var baseY = (float)Math.Sin(a) * Radius;
            var y = baseY * Squash - x * Tilt;
            v.Add(Center + new Vector2(x, y - z));
        }

        return v;
    }
}

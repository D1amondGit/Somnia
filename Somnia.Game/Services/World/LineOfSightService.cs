using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;

namespace Somnia.Game.Services.World;

/// <summary>Проверка прямой видимости: луч пересекает полигон «крышки» стены (<see cref="HexagonModel.GetTopVertices"/>).</summary>
public interface ILineOfSightService
{
    bool HasLineOfSight(Vector2 from, Vector2 to, IReadOnlyList<HexagonModel> walls);
}

public sealed class LineOfSightService : ILineOfSightService
{
    public bool HasLineOfSight(Vector2 from, Vector2 to, IReadOnlyList<HexagonModel> walls)
    {
        foreach (var w in walls)
        {
            var top = w.GetTopVertices();
            if (PhysicsHelper.SegmentIntersectsPolygon(from, to, top))
                return false;
        }

        return true;
    }
}

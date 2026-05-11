using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class ResourceDropModel
{
    public Vector2 Position { get; private set; }
    public DropType Type { get; }
    public float Value { get; }
    public bool Collected { get; private set; }

    public ResourceDropModel(Vector2 pos, DropType type, float value)
    {
        Position = pos;
        Type = type;
        Value = value;
    }

    public void Update(Vector2 playerPos, float dt)
    {
        if (Collected || dt <= 0) return;

        float dist = Vector2.Distance(Position, playerPos);
        if (dist < 25f)
        {
            Collected = true;
            return;
        }

        if (dist < 150f && dist > 0.001f)
        {
            Vector2 dir = Vector2.Normalize(playerPos - Position);
            Position += dir * 250f * dt;
        }
    }
}

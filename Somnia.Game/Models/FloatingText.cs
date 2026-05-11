using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public sealed class FloatingText
{
    public Vector2 Position;
    public string Text = "";
    public Color Color;
    public float Timer = 1f;
}

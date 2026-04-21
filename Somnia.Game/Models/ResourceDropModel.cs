using System.Numerics;

namespace Somnia.Game.Models
{
    public enum DropType { Health, Mana }

    public class ResourceDropModel
    {
        public Vector2 Position { get; set; }
        public DropType Type { get; }
        public float Value { get; }
        public bool Collected { get; private set; }

        private const float AttractionRadius = 120f;
        private const float CollectRadius = 20f;

        public ResourceDropModel(Vector2 pos, DropType type, float value)
        {
            Position = pos;
            Type = type;
            Value = value;
        }

        public void Update(Vector2 playerPos)
        {
            float dist = Vector2.Distance(Position, playerPos);
            if (dist < AttractionRadius)
                Position = Vector2.Lerp(Position, playerPos, 0.12f);
            if (dist < CollectRadius)
                Collected = true;
        }
    }
}

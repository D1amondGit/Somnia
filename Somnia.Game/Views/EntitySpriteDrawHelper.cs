using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Somnia.Game.Views;

/// <summary>Отрисовка спрайта «ногами» в точке мира, масштаб по высоте, зеркало по X.</summary>
public static class EntitySpriteDrawHelper
{
    public static void DrawBottomCenter(
        SpriteBatch sb,
        Texture2D tex,
        Vector2 footWorld,
        float targetHeightPx,
        Color tint,
        bool flipHorizontal,
        float bottomPaddingFrac = 0f)
    {
        if (tex.Height <= 0) return;
        var scale = targetHeightPx / tex.Height;
        var frac = MathHelper.Clamp(bottomPaddingFrac, 0f, 0.55f);
        var originY = tex.Height * (1f - frac);
        var origin = new Vector2(tex.Width / 2f, originY);
        var effects = flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        sb.Draw(tex, footWorld, null, tint, 0f, origin, scale, effects, 0f);
    }

    /// <summary>Тот же спрайт + тёмный контур (8 смещений) — лучше читается на тёмном полу.</summary>
    public static void DrawBottomCenterOutlined(
        SpriteBatch sb,
        Texture2D tex,
        Vector2 footWorld,
        float targetHeightPx,
        Color tint,
        bool flipHorizontal,
        float outlinePx = 2.25f,
        float outlineAlpha = 0.58f,
        float bottomPaddingFrac = 0f)
    {
        if (tex.Height <= 0) return;
        var scale = targetHeightPx / tex.Height;
        var frac = MathHelper.Clamp(bottomPaddingFrac, 0f, 0.55f);
        var originY = tex.Height * (1f - frac);
        var origin = new Vector2(tex.Width / 2f, originY);
        var effects = flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var outlineA = (byte)MathHelper.Clamp(outlineAlpha * 255f, 40f, 220f);
        var outline = new Color((byte)0, (byte)0, (byte)0, outlineA);

        for (var ox = -1; ox <= 1; ox++)
        for (var oy = -1; oy <= 1; oy++)
        {
            if (ox == 0 && oy == 0) continue;
            var off = new Vector2(ox, oy) * outlinePx;
            sb.Draw(tex, footWorld + off, null, outline, 0f, origin, scale, effects, 0f);
        }

        sb.Draw(tex, footWorld, null, tint, 0f, origin, scale, effects, 0f);
    }
}

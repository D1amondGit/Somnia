using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>Цветовая схема игрока: зависит от текущей зоны и выбранного слота.</summary>
public static class PlayerPalette
{
    public static Color GetBodyColor(PlayerModel player)
    {
        var (basic, mid, ulti) = ZoneColors(player.CurrentZone);
        return player.ActiveSlot switch
        {
            0 => basic,
            1 => mid,
            2 => ulti,
            _ => basic
        };
    }

    public static Color GetAccentColor(PlayerModel player)
    {
        var body = GetBodyColor(player);
        if (player.IsDashing) return Color.White;
        if (player.GreenAuraTimer > 0) return Color.LimeGreen;
        return Brighten(body, 0.35f);
    }

    public static Color GetZoneTint(AnomalyType zone) =>
        zone switch
        {
            AnomalyType.Red => new Color(255, 80, 80),
            AnomalyType.Blue => new Color(110, 160, 255),
            AnomalyType.Green => new Color(110, 230, 140),
            _ => new Color(200, 200, 210)
        };

    private static (Color Basic, Color Mid, Color Ult) ZoneColors(AnomalyType z) =>
        z switch
        {
            AnomalyType.Red => (new Color(220, 70, 50), new Color(255, 130, 70), new Color(255, 200, 70)),
            AnomalyType.Blue => (new Color(80, 130, 240), new Color(120, 200, 255), new Color(180, 230, 255)),
            AnomalyType.Green => (new Color(90, 200, 110), new Color(150, 230, 100), new Color(200, 255, 120)),
            _ => (new Color(190, 190, 210), new Color(220, 220, 230), new Color(240, 240, 250))
        };

    private static Color Brighten(Color c, float amount) =>
        new(
            (byte)System.Math.Min(255, c.R + 255 * amount),
            (byte)System.Math.Min(255, c.G + 255 * amount),
            (byte)System.Math.Min(255, c.B + 255 * amount));
}

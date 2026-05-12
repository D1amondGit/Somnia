using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>Описание одной «кнопки» скилла — название и тип иконки для рендера.</summary>
public readonly record struct SkillSlotIcon(string Title, SkillIconShape Icon, Color Tint);

public enum SkillIconShape
{
    None,
    Rifle,
    Shotgun,
    Sniper,
    Grenade,
    Aura,
    Dash,
    Bomb,
    Slow,
    Beam,
    Pull,
    Infect
}

/// <summary>
/// Каталог иконок: что показывать в HUD для (зона, слот). Никакой логики — только данные.
/// </summary>
public static class SkillSlotCatalog
{
    public static SkillSlotIcon Get(AnomalyType zone, int slot)
    {
        return (zone, slot) switch
        {
            (AnomalyType.Neutral, 0) => new("AUTO", SkillIconShape.Rifle, new Color(220, 220, 240)),
            (AnomalyType.Red, 0) => new("SHOTGUN", SkillIconShape.Shotgun, new Color(255, 110, 80)),
            (AnomalyType.Blue, 0) => new("SNIPER", SkillIconShape.Sniper, new Color(120, 190, 255)),
            (AnomalyType.Green, 0) => new("GRENADE", SkillIconShape.Grenade, new Color(130, 230, 130)),

            (AnomalyType.Red, 1) => new("PULL", SkillIconShape.Pull, new Color(255, 130, 90)),
            (AnomalyType.Red, 2) => new("BLAST", SkillIconShape.Bomb, new Color(255, 90, 60)),

            (AnomalyType.Blue, 1) => new("DASH", SkillIconShape.Dash, new Color(140, 200, 255)),
            (AnomalyType.Blue, 2) => new("FREEZE", SkillIconShape.Slow, new Color(160, 220, 255)),

            (AnomalyType.Green, 1) => new("SHIELD", SkillIconShape.Aura, new Color(120, 230, 130)),
            (AnomalyType.Green, 2) => new("INFECT", SkillIconShape.Infect, new Color(160, 240, 160)),

            (AnomalyType.Neutral, 1) => new("—", SkillIconShape.None, new Color(120, 120, 130)),
            (AnomalyType.Neutral, 2) => new("—", SkillIconShape.None, new Color(120, 120, 130)),

            _ => new("?", SkillIconShape.None, Color.Gray)
        };
    }
}

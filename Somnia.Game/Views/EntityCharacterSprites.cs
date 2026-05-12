using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Somnia.Game.Views;

/// <summary>Опциональные PNG для игрока, NPC и врагов. При отсутствии файла слот остаётся null — рендер векторный.</summary>
public sealed class EntityCharacterSprites
{
    public Texture2D? PlayerStay { get; private set; }
    public Texture2D? PlayerWalk { get; private set; }
    public Texture2D? PlayerCarryStay { get; private set; }
    public Texture2D? PlayerCarryWalk { get; private set; }
    public Texture2D? Npc { get; private set; }
    public Texture2D? Boss { get; private set; }
    public Texture2D? MeleeWalk1 { get; private set; }
    public Texture2D? MeleeWalk2 { get; private set; }
    public Texture2D? SniperStay { get; private set; }
    public Texture2D? SniperWalk { get; private set; }

    public bool HasPlayerSprites =>
        PlayerStay != null || PlayerWalk != null || PlayerCarryStay != null || PlayerCarryWalk != null;

    public void LoadContent(ContentManager content)
    {
        PlayerStay = TryTex(content, "Player/player-stay");
        PlayerWalk = TryTex(content, "Player/player-walk");
        PlayerCarryStay = TryTex(content, "Carry/player-carry-stay");
        PlayerCarryWalk = TryTex(content, "Carry/player-carry-walk");
        Npc = TryTex(content, "Npc/npc");
        Boss = TryTex(content, "Enemies/boss");
        MeleeWalk1 = TryTex(content, "Enemies/melee-walk-1");
        MeleeWalk2 = TryTex(content, "Enemies/melee-walk-2");
        SniperStay = TryTex(content, "Enemies/sniper-stay");
        SniperWalk = TryTex(content, "Enemies/sniper-walk");
    }

    private static Texture2D? TryTex(ContentManager content, string asset)
    {
        try
        {
            return content.Load<Texture2D>(asset);
        }
        catch
        {
            return null;
        }
    }
}

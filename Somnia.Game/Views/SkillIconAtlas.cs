using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;

namespace Somnia.Game.Views;

/// <summary>PNG-иконки скиллов из Content/Skills/*. Иначе — векторные заглушки <see cref="SkillIconView"/>.</summary>
public sealed class SkillIconAtlas
{
    private readonly Dictionary<SkillIconShape, Texture2D> _map = new();

    public Texture2D? Get(SkillIconShape s) => _map.TryGetValue(s, out var t) ? t : null;

    public void LoadContent(ContentManager content)
    {
        TryLoad(content, "Skills/auto", SkillIconShape.Rifle);
        TryLoad(content, "Skills/shotgun", SkillIconShape.Shotgun);
        TryLoad(content, "Skills/sniper", SkillIconShape.Sniper);
        TryLoad(content, "Skills/grenade", SkillIconShape.Grenade);
        TryLoad(content, "Skills/shield", SkillIconShape.Aura);
        TryLoad(content, "Skills/dash", SkillIconShape.Dash);
        TryLoad(content, "Skills/bomb", SkillIconShape.Bomb);
        TryLoad(content, "Skills/freeze", SkillIconShape.Slow);
        TryLoad(content, "Skills/pull", SkillIconShape.Pull);
        TryLoad(content, "Skills/infect", SkillIconShape.Infect);
    }

    private void TryLoad(ContentManager content, string asset, SkillIconShape shape)
    {
        try
        {
            var t = content.Load<Texture2D>(asset);
            if (t != null) _map[shape] = t;
        }
        catch
        {
        }
    }
}

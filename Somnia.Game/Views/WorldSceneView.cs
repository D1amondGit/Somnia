using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Somnia.Game.Models;
using Somnia.Game.Views.Floor;
using Somnia.Game.Views.Rendering;

namespace Somnia.Game.Views;

public sealed class WorldSceneView
{
    private readonly SpritePrimitiveRenderer _prim;

    public WorldSceneView(GraphicsDevice device) => _prim = new SpritePrimitiveRenderer(device);

    public void Draw(
        SpriteBatch sb,
        Rectangle playArea,
        int layoutSeed,
        PlayerModel player,
        IReadOnlyList<EnemyModel> enemies,
        IReadOnlyList<AnomalyZone> zones,
        NpcModel npc,
        IReadOnlyList<HexagonModel> walls,
        Texture2D? wallTexture,
        IReadOnlyList<ResourceDropModel> drops,
        IReadOnlyList<GateModel> gates,
        IReadOnlyList<FloatingText> floatingTexts,
        SpriteFont? font,
        IReadOnlyList<ProjectileModel> enemyProj,
        IReadOnlyList<PlayerProjectileModel> playerProj)
    {
        LargeHexFloorRenderer.Draw(sb, _prim, playArea, layoutSeed);

        foreach (var z in zones)
        {
            var col = SpritePrimitiveRenderer.ZoneFlashColor(z.Type);
            _prim.FillPoly(sb, z.Outline, col * 0.22f);
            DrawClosedOutline(sb, z.Outline, col * 0.55f);
        }

        foreach (var w in walls)
            SpritePrimitiveRenderer.DrawHexWalls(sb, _prim, w, wallTexture);

        foreach (var w in walls)
            _prim.FillPoly(sb, w.GetTopVertices(), Color.Black);

        foreach (var g in gates)
        {
            var h = new HexagonModel(g.Position, 80f, 40f, IsometricView.Squash, IsometricView.Tilt);
            _prim.FillPoly(sb, h.GetTopVertices(), g.IsOpen ? Color.LimeGreen : Color.Red * 0.5f);
        }

        DrawOutskirts(sb, playArea);

        foreach (var d in drops)
            _prim.DrawCircleOutline(sb, d.Position, 6f, d.Type == DropType.Health ? Color.Red : Color.Cyan, 5);

        foreach (var ep in enemyProj)
            _prim.DrawCircleOutline(sb, ep.Position, ep.Radius, Color.Orange, 4);

        foreach (var pp in playerProj)
        {
            var c = pp.Kind switch
            {
                PlayerProjectileKind.Rocket => Color.OrangeRed,
                PlayerProjectileKind.Pellet => Color.White,
                _ => Color.LightYellow
            };

            _prim.DrawCircleOutline(sb, pp.Position, pp.Radius + 2f, c, 4);
            _prim.DrawLine(sb, pp.Position - pp.Velocity * 0.02f, pp.Position + pp.Velocity * 0.045f, c * 0.9f, 3);
        }

        DrawPlayer(sb, player);
        foreach (var e in enemies)
        {
            if (!e.IsDead) DrawEnemy(sb, e);
        }

        if (!npc.IsPickedUp && !npc.IsDead)
            DrawNpc(sb, npc);

        if (player.GreenAuraTimer > 0)
            _prim.DrawCircleOutline(sb, player.Position, 200f, Color.LimeGreen, 3);

        if (player.IsAttacking)
            DrawAttackPreview(sb, player);

        if (font != null)
        {
            foreach (var t in floatingTexts)
                sb.DrawString(font, t.Text, t.Position + new Vector2(-20, -50), t.Color * t.Timer);
        }
    }

    private void DrawClosedOutline(SpriteBatch sb, IReadOnlyList<Vector2> verts, Color color)
    {
        for (var i = 0; i < verts.Count; i++)
            _prim.DrawLine(sb, verts[i], verts[(i + 1) % verts.Count], color, thickness: 2);
    }

    private void DrawOutskirts(SpriteBatch sb, Rectangle p)
    {
        var t = _prim.PixelTexture;

        sb.Draw(t, new Rectangle(-2000, -2000, 8000, p.Y + 2000), Color.Black);
        sb.Draw(t, new Rectangle(-2000, p.Bottom, 8000, 2000), Color.Black);
        sb.Draw(t, new Rectangle(-2000, p.Y, p.X + 2000, p.Height), Color.Black);
        sb.Draw(t, new Rectangle(p.Right, p.Y, 2000, p.Height), Color.Black);
    }

    private void DrawPlayer(SpriteBatch sb, PlayerModel p)
    {
        sb.Draw(_prim.PixelTexture,
            new Rectangle((int)p.Position.X - 25, (int)p.Position.Y - 25, 50, 50),
            p.IsDashing ? Color.Cyan : (p.State == PlayerState.Free ? Color.Blue : Color.Green));
    }

    private void DrawNpc(SpriteBatch sb, NpcModel npc)
    {
        sb.Draw(_prim.PixelTexture,
            new Rectangle((int)npc.Position.X - 20, (int)npc.Position.Y - 20, 40, 40), Color.Yellow);
        sb.Draw(_prim.PixelTexture,
            new Rectangle((int)npc.Position.X - 20, (int)npc.Position.Y - 30,
                (int)(40 * (npc.Health / npc.MaxHealth)), 5), Color.LimeGreen);
    }

    private void DrawEnemy(SpriteBatch sb, EnemyModel e)
    {
        Color eCol =
            e.DamageFlash > 0
                ? Color.White
                : (e.StunTimer > 0
                    ? Color.LightGray
                    : (e.SlowTimer > 0 ? Color.CornflowerBlue : Color.Purple));

        sb.Draw(_prim.PixelTexture,
            new Rectangle((int)e.Position.X - 20, (int)e.Position.Y - 20, 40, 40), eCol);
        sb.Draw(_prim.PixelTexture,
            new Rectangle((int)e.Position.X - 20, (int)e.Position.Y - 30,
                (int)(40 * (e.Health / e.MaxHealth)), 5), Color.Red);
    }

    private void DrawAttackPreview(SpriteBatch sb, PlayerModel p)
    {
        var cPos = p.Position;

        switch (p.CurrentZone)
        {
            case AnomalyType.Neutral when p.ActiveSlot == 0:
                _prim.DrawLine(sb, cPos, cPos + p.FacingDir * 800f, Color.White, 8);
                break;
            case AnomalyType.Green when p.ActiveSlot == 0:
                _prim.DrawLine(sb, cPos, cPos + p.FacingDir * 1000f, Color.Lime, 15);
                break;
            case AnomalyType.Red when p.ActiveSlot == 0:
                _prim.DrawCone(sb, cPos, p.FacingDir, 250f, 0.6f, Color.Red);
                break;
            case AnomalyType.Red when p.ActiveSlot == 2:
                _prim.DrawCircleOutline(sb, cPos, 300f, Color.Red, 10);
                break;
            case AnomalyType.Blue when p.ActiveSlot == 1:
                _prim.DrawCircleOutline(sb, cPos, 250f, Color.Blue, 5);
                break;
        }
    }
}

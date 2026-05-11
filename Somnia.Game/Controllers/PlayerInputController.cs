using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Somnia.Game.Models;
using Somnia.Game.Services.Combat;
using Somnia.Game.Services.Npc;

namespace Somnia.Game.Controllers;

public sealed class PlayerInputController
{
    private readonly PlayerModel _player;
    private readonly IPlayerCombatService _combat;
    private MouseState _prevM;
    private KeyboardState _prevK;

    public PlayerInputController(PlayerModel player, IPlayerCombatService combat)
    {
        _player = player;
        _combat = combat;
    }

    /// <summary>Читает устройство ввода, движение игрока и запрос активного навыка.</summary>
    public void Update(
        GameplayFrameContext ctx)
    {
        var ms = Mouse.GetState();
        var ks = Keyboard.GetState();

        var worldM = Vector2.Transform(new Vector2(ms.X, ms.Y), Matrix.Invert(ctx.Camera));
        var toMouse = worldM - _player.Position;

        _player.UpdateFacing(toMouse);

        if (ks.IsKeyDown(Keys.LeftShift) && _prevK.IsKeyUp(Keys.LeftShift) &&
            _player.State == PlayerState.Free)
            _player.StartDash();

        if (ks.IsKeyDown(Keys.D1)) _player.ActiveSlot = 0;
        if (ks.IsKeyDown(Keys.D2)) _player.ActiveSlot = 1;
        if (ks.IsKeyDown(Keys.D3)) _player.ActiveSlot = 2;

        if (ms.LeftButton == ButtonState.Pressed && _prevM.LeftButton == ButtonState.Released)
        {
            NpcCarryInteractionService.DropCarriedNpc(_player, ctx.Npc);
            _combat.TryUseActiveSkill(_player, worldM,
                ctx.Enemies, ctx.Npc, ctx.Walls, ctx.PlayerProjectiles);
        }

        float speed;

        speed = ctx.BaseSpeedMultiplier * (_player.State == PlayerState.Carrying
            ? PlayerModel.SpeedCarrying
            : PlayerModel.SpeedFree);
        if (_player.IsDashing) speed = ctx.BaseSpeedMultiplier * PlayerModel.SpeedDashing;

        var dir = GetKeyboardDirection(ks);
        if (dir != Vector2.Zero)
        {
            dir.Normalize();
            _player.Position += dir * speed * ctx.DeltaTime;
        }

        _player.Position.X =
            MathHelper.Clamp(_player.Position.X, 100f, ctx.MapWidth - 100f);

        _player.Position.Y =
            MathHelper.Clamp(_player.Position.Y, 100f, ctx.MapHeight - 100f);

        _prevM = ms;
        _prevK = ks;
    }

    /// <summary>Множитель скорости (например кастомные бафы/дебафы снаружи).</summary>
    private static Vector2 GetKeyboardDirection(KeyboardState s)
    {
        var d = Vector2.Zero;
        if (s.IsKeyDown(Keys.W)) d.Y -= 1;
        if (s.IsKeyDown(Keys.S)) d.Y += 1;
        if (s.IsKeyDown(Keys.A)) d.X -= 1;
        if (s.IsKeyDown(Keys.D)) d.X += 1;
        return d;
    }
}

/// <summary>Все необходимое для одиночного игрового кадра перемещений/скилла.</summary>
public sealed record GameplayFrameContext(
    float DeltaTime,
    int MapWidth,
    int MapHeight,
    Matrix Camera,
    List<EnemyModel> Enemies,
    NpcModel Npc,
    List<HexagonModel> Walls,
    List<PlayerProjectileModel> PlayerProjectiles,
    float BaseSpeedMultiplier = 1f);

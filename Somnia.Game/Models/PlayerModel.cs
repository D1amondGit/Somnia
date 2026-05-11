using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class PlayerModel
{
    public const float SpeedFree = 300f;
    public const float SpeedCarrying = 150f;
    public const float SpeedDashing = 800f;

    public Vector2 Position, FacingDir = Vector2.UnitX;
    public PlayerState State { get; private set; } = PlayerState.Free;
    public AnomalyType CurrentZone = AnomalyType.Neutral;
    public float CurrentHealth = 100f, MaxHealth = 100f, CurrentMana = 100f, MaxMana = 100f, DamageMultiplier = 1.0f;

    public bool IsDead => CurrentHealth <= 0;
    public int ActiveSlot;

    public float Cd1, Cd2, Cd3;
    public float MaxCd1 = 0.5f, MaxCd2 = 2f, MaxCd3 = 5f;

    public bool IsDashing => _dashTimer > 0;
    public bool IsAttacking => _attackTimer > 0;
    public float GreenAuraTimer { get; private set; }

    private float _dashTimer, _dashCd, _attackTimer;

    public PlayerModel(Vector2 start) => Position = start;

    public void SetState(PlayerState s) => State = s;

    public void TakeDamage(float a) => CurrentHealth = MathHelper.Max(0, CurrentHealth - a);

    public void UpdateFacing(Vector2 d)
    {
        if (d == Vector2.Zero) return;
        d.Normalize();
        FacingDir = d;
    }

    public bool StartDash()
    {
        if (_dashCd > 0) return false;
        _dashTimer = 0.2f;
        _dashCd = 1f;
        return true;
    }

    /// <summary>Рывок из навыка (не блокируется кулдауном обычного dash).</summary>
    public void ActivateSkillForcedDash()
    {
        _dashTimer = 0.2f;
        _dashCd = 1f;
    }

    public bool ConsumeMana(float a)
    {
        if (CurrentMana < a) return false;
        CurrentMana -= a;
        return true;
    }

    public void TickCooldowns(float dt)
    {
        CurrentMana = MathHelper.Min(MaxMana, CurrentMana + 10f * dt);
        if (Cd1 > 0) Cd1 -= dt;
        if (Cd2 > 0) Cd2 -= dt;
        if (Cd3 > 0) Cd3 -= dt;
        if (_dashCd > 0) _dashCd -= dt;
        if (_dashTimer > 0) _dashTimer -= dt;
        if (_attackTimer > 0) _attackTimer -= dt;
    }

    /// <summary>Обновляет ульт тури зелёной зоны (тик урона).</summary>
    public void TickGreenAura(float dt, System.Collections.Generic.IReadOnlyList<EnemyModel> enemies)
    {
        if (GreenAuraTimer <= 0) return;

        GreenAuraTimer -= dt;
        foreach (var e in enemies)
        {
            if (e.IsDead || Vector2.Distance(Position, e.Position) >= 200f) continue;
            Vector2 push = e.Position - Position;
            if (push == Vector2.Zero) continue;

            e.Position += Vector2.Normalize(push) * 400f * dt;
            e.TakeDamage(15f * dt, Position, 0f);
        }
    }

    /// <summary>Выставляет таймер визуала атаки и кулдауны активного слота при успешном касте.</summary>
    public void RegisterSkillExecuted()
    {
        _attackTimer = 0.25f;
        if (ActiveSlot == 0) Cd1 = MaxCd1;
        else if (ActiveSlot == 1) Cd2 = MaxCd2;
        else Cd3 = MaxCd3;
    }

    public bool SkillOnCooldown =>
        ActiveSlot switch
        {
            0 => Cd1 > 0,
            1 => Cd2 > 0,
            2 => Cd3 > 0,
            _ => true
        };

    public void BeginGreenAura(float durationSeconds) => GreenAuraTimer = durationSeconds;

    /// <summary>Полный сброс при рестарте / новой арене (HP, мана, КД, ауры, таймеры).</summary>
    public void ResetForRun()
    {
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;
        DamageMultiplier = 1f;
        CurrentZone = AnomalyType.Neutral;
        ActiveSlot = 0;
        Cd1 = Cd2 = Cd3 = 0f;
        GreenAuraTimer = 0f;
        _dashTimer = 0f;
        _dashCd = 0f;
        _attackTimer = 0f;
    }
}

using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class PlayerModel
{
    public const float SpeedFree = 430f;
    public const float SpeedCarrying = 320f;
    public const float SpeedDashing = 1100f;
    public const float ManaRegenPerSec = 18f;

    /// <summary>Круг столкновения в плоскости мира (совпадает с «ногами» под спрайтом).</summary>
    public const float CollisionRadius = 32f;

    /// <summary>Доля высоты PNG снизу — пустой паддинг под персонажем; якорь поднимается, чтобы ноги совпали с <see cref="Position"/>.</summary>
    public const float SpriteBottomPaddingFrac = 0.34f;

    public Vector2 Position, FacingDir = Vector2.UnitX;

    /// <summary>Доп. импульс (отдача от выстрелов, толчки). Затухает в TickCooldowns.</summary>
    public Vector2 KnockbackVelocity;

    public PlayerState State { get; set; } = PlayerState.Free;
    public AnomalyType CurrentZone = AnomalyType.Neutral;
    public float CurrentHealth = 100f, MaxHealth = 100f, CurrentMana = 100f, MaxMana = 100f, DamageMultiplier = 1.0f;

    public bool IsDead => CurrentHealth <= 0;
    public int ActiveSlot;

    public float Cd1, Cd2, Cd3;
    public float MaxCd1 = 0.3f, MaxCd2 = 1.4f, MaxCd3 = 3.5f;

    public bool IsDashing => _dashTimer > 0;
    public bool IsAttacking => _attackTimer > 0;

    /// <summary>Срабатывает от зелёного слота 2 (бывший «green aura»): щит отталкивает врагов
    /// и поглощает <see cref="ShieldDamageReduction"/> входящего урона, пока активен.</summary>
    public float ShieldTimer { get; private set; }

    public float ShieldRadius { get; private set; } = 220f;
    public float ShieldDamageReduction { get; private set; } = 0.65f;
    public bool IsShieldActive => ShieldTimer > 0f;

    /// <summary>Совместимая обёртка: старое имя «GreenAuraTimer» теперь — таймер щита.</summary>
    public float GreenAuraTimer => ShieldTimer;

    private float _dashTimer, _dashCd, _attackTimer;

    /// <summary>Увеличивается при каждом успешном касте навыка (для SFX выстрела и т.п.).</summary>
    public int SkillFireCount { get; private set; }

    public PlayerModel(Vector2 start) => Position = start;

    public void SetState(PlayerState s) => State = s;

    public void TakeDamage(float a)
    {
        if (IsShieldActive) a *= 1f - ShieldDamageReduction;
        CurrentHealth = MathHelper.Max(0, CurrentHealth - a);
    }

    public void Heal(float a) => CurrentHealth = MathHelper.Min(MaxHealth, CurrentHealth + a);

    /// <summary>Толчок (отдача оружия, телекинез и т.п.).</summary>
    public void ApplyKnockback(Vector2 impulse) => KnockbackVelocity += impulse;

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
        CurrentMana = MathHelper.Min(MaxMana, CurrentMana + ManaRegenPerSec * dt);
        if (Cd1 > 0) Cd1 -= dt;
        if (Cd2 > 0) Cd2 -= dt;
        if (Cd3 > 0) Cd3 -= dt;
        if (_dashCd > 0) _dashCd -= dt;
        if (_dashTimer > 0) _dashTimer -= dt;
        if (_attackTimer > 0) _attackTimer -= dt;
        if (ShieldTimer > 0) ShieldTimer -= dt;

        Position += KnockbackVelocity * dt;
        var decay = MathHelper.Clamp(1f - 6f * dt, 0f, 1f);
        KnockbackVelocity *= decay;
    }

    /// <summary>Каждый тик щита: отталкивает врагов в радиусе наружу. Урон не наносит — щит «защитный».</summary>
    public void TickGreenAura(float dt, System.Collections.Generic.IReadOnlyList<EnemyModel> enemies)
    {
        if (!IsShieldActive) return;
        foreach (var e in enemies)
        {
            if (e.IsDead || Vector2.Distance(Position, e.Position) >= ShieldRadius) continue;
            var push = e.Position - Position;
            if (push == Vector2.Zero) continue;
            e.Position += Vector2.Normalize(push) * 500f * dt;
            e.Velocity += Vector2.Normalize(push) * 60f;
        }
    }

    /// <summary>Выставляет таймер визуала атаки и кулдауны активного слота при успешном касте.</summary>
    public void RegisterSkillExecuted()
    {
        SkillFireCount++;
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

    /// <summary>Включить щит: входящий урон режется на <see cref="ShieldDamageReduction"/>,
    /// активирующий импульс выталкивает врагов в радиусе наружу.</summary>
    public void BeginShield(float durationSeconds, float radius = 220f, float reduction = 0.65f)
    {
        ShieldTimer = durationSeconds;
        ShieldRadius = radius;
        ShieldDamageReduction = MathHelper.Clamp(reduction, 0f, 0.9f);
    }

    /// <summary>Совместимость со старым именем (старые тесты/код вызывают BeginGreenAura).</summary>
    public void BeginGreenAura(float durationSeconds) => BeginShield(durationSeconds);

    /// <summary>Полный сброс при рестарте / новой арене (HP, мана, КД, ауры, таймеры).</summary>
    public void ResetForRun()
    {
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;
        DamageMultiplier = 1f;
        CurrentZone = AnomalyType.Neutral;
        ActiveSlot = 0;
        Cd1 = Cd2 = Cd3 = 0f;
        ShieldTimer = 0f;
        KnockbackVelocity = Vector2.Zero;
        _dashTimer = 0f;
        _dashCd = 0f;
        _attackTimer = 0f;
        SkillFireCount = 0;
    }
}

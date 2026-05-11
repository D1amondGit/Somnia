using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class EnemyModel
{
    public Vector2 Position, Velocity;
    public float Health = 100f, MaxHealth = 100f;
    public EnemyType Type;
    public EnemyArchetype Archetype { get; }

    public float StunTimer, SlowTimer, AttackCooldown, InfectionTimer, DamageFlash;
    public float TelegraphTimer;
    public bool TelegraphArmed;
    public bool IsInfected, IsDummy, HasDropped;
    public bool IsDead => Health <= 0;
    public bool IsTelegraphing => TelegraphTimer > 0;

    /// <summary>Короткий «вспышечный» таймер у дула — для рендера muzzle flash сразу после выстрела.</summary>
    public float MuzzleFlashTimer;
    public Vector2 MuzzleFlashDir;

    /// <summary>Внутреннее состояние босса (см. <see cref="Models.BossAttackPhase"/>).</summary>
    public BossAttackPhase BossPhase;

    /// <summary>Таймер текущей фазы (телеграф/исполнение/откат).</summary>
    public float BossPhaseTimer;

    /// <summary>Центр текущей AoE-атаки босса (используется для slam-телеграфа и удара).</summary>
    public Vector2 BossActionCenter;

    /// <summary>Радиус AoE-атаки.</summary>
    public float BossActionRadius;

    /// <summary>После первого успешного slam — для чуть более длинного телеграфа только на первый удар.</summary>
    public bool HasCompletedFirstBossSlam;

    /// <summary>Босс: в этом цикле idle→атака приоритетно давит на NPC (слэм/залп/рывок в его сторону).</summary>
    public bool BossFocusOnNpc;

    /// <summary>Счётчик для стабильного реролла агро босса.</summary>
    public int BossAggroRollNonce;

    public EnemyModel(Vector2 pos, EnemyType type = EnemyType.Melee)
    {
        Position = pos;
        Type = type;
        Archetype = EnemyArchetypeCatalog.Get(type);
        MaxHealth = Archetype.MaxHealth;
        Health = MaxHealth;

        if (type == EnemyType.Boss)
        {
            BossPhase = BossAttackPhase.Idle;
            // Даём время разойтись с NPC до первого паттерна (раньше slam стартовал в кадр 0).
            BossPhaseTimer = 4.2f;
        }
    }

    public void TakeDamage(float dmg, Vector2 src, float kb)
    {
        Health -= dmg;
        DamageFlash = 0.15f;
        var dir = Position - src;
        if (dir != Vector2.Zero && kb > 0)
        {
            dir.Normalize();
            Velocity += dir * kb;
        }
    }

    public void Update(float dt)
    {
        Position += Velocity * dt;
        Velocity = Vector2.Lerp(Velocity, Vector2.Zero, 0.1f);
        if (AttackCooldown > 0) AttackCooldown -= dt;
        if (StunTimer > 0) StunTimer -= dt;
        if (SlowTimer > 0) SlowTimer -= dt;
        if (DamageFlash > 0) DamageFlash -= dt;
        if (TelegraphTimer > 0) TelegraphTimer -= dt;
        if (MuzzleFlashTimer > 0) MuzzleFlashTimer -= dt;
    }
}

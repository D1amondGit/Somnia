using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

public class EnemyModel
{
    public Vector2 Position, Velocity;
    public float Health = 100f, MaxHealth = 100f;
    public EnemyType Type;
    public float StunTimer, SlowTimer, AttackCooldown, InfectionTimer, DamageFlash;
    public bool IsInfected, IsDummy, HasDropped;
    public bool IsDead => Health <= 0;

    public EnemyModel(Vector2 pos, EnemyType type = EnemyType.Melee)
    {
        Position = pos;
        Type = type;
    }

    public void TakeDamage(float dmg, Vector2 src, float kb)
    {
        Health -= dmg;
        DamageFlash = 0.15f;
        Vector2 dir = Position - src;
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
    }
}

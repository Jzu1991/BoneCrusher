using UnityEngine;

/// <summary>Zombi (Piso 27) - Lento pero resistente.</summary>
public class ZombieAI : EnemyAI
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed      = 1.2f;
        attackCooldown = 2f;
        attackDamage   = 1;
    }

    protected override void Chase()
    {
        base.Chase();
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed),
            rb.linearVelocity.y
        );
    }
}

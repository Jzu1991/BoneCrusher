using UnityEngine;

/// <summary>Guerrero Aventurero (Piso 30) - Jefe. Se acelera al 40% HP.</summary>
public class WarriorAdventurerAI : EnemyAI
{
    [Header("Guerrero Aventurero")]
    [SerializeField] private float chargeSpeed     = 7f;
    [SerializeField] private float chargeThreshold = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed      = 2.5f;
        attackCooldown = 1.8f;
        attackDamage   = 1;
        detectionRange = 16f;
        attackRange    = 1.2f;
    }

    protected override void Chase()
    {
        float hpRatio    = (float)health.CurrentHP / health.MaxHP;
        float speed      = hpRatio <= chargeThreshold ? chargeSpeed : moveSpeed;
        float dir        = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(speed * dir, rb.linearVelocity.y);
        if (dir > 0 && !facingRight) Flip();
        if (dir < 0 &&  facingRight) Flip();
    }
}

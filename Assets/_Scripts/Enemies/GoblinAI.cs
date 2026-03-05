using UnityEngine;

/// <summary>Goblin (Piso 29) - Rapido y agresivo, embiste ocasionalmente.</summary>
public class GoblinAI : EnemyAI
{
    [Header("Goblin")]
    [SerializeField] private float dashSpeed  = 6f;
    [SerializeField] private float dashChance = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed      = 3.5f;
        attackCooldown = 1.2f;
        attackDamage   = 1;
        detectionRange = 12f;
    }

    protected override void Chase()
    {
        if (Random.value < dashChance * Time.deltaTime)
        {
            float dir = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashSpeed * dir, rb.linearVelocity.y);
        }
        else
        {
            base.Chase();
        }
    }
}

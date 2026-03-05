using UnityEngine;

/// <summary>Picaro Aventurero (Piso 25) - Jefe final demo. Ataca y retrocede.</summary>
public class RogueAdventurerAI : EnemyAI
{
    [Header("Picaro Aventurero")]
    [SerializeField] private float backstepDistance = 2f;
    [SerializeField] private float backstepSpeed    = 5f;

    private bool  isBackstepping = false;
    private float backstepTimer;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed      = 4f;
        attackCooldown = 1f;
        attackDamage   = 1;
        detectionRange = 16f;
        attackRange    = 1f;
    }

    protected override void Chase()
    {
        if (isBackstepping) return;
        base.Chase();
    }

    protected override void Attack()
    {
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            animator?.SetTrigger("Attack");
            DealDamage();
            isBackstepping = true;
            backstepTimer  = 0.4f;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isBackstepping)
        {
            backstepTimer -= Time.deltaTime;
            float dir = (player != null && player.position.x > transform.position.x) ? -1f : 1f;
            rb.linearVelocity = new Vector2(backstepSpeed * dir, rb.linearVelocity.y);
            if (backstepTimer <= 0f) isBackstepping = false;
        }
    }
}

using UnityEngine;

/// <summary>Esqueleto Arquero (Piso 28) - Ataca a distancia, huye en melee.</summary>
public class SkeletonArcherAI : EnemyAI
{
    [Header("Arquero")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform  bowPoint;
    [SerializeField] private float      preferredRange = 4f;
    [SerializeField] private float      fleeRange      = 2f;

    protected override void Awake()
    {
        base.Awake();
        attackRange    = 5f;
        detectionRange = 14f;
        moveSpeed      = 2f;
        attackCooldown = 2f;
        attackDamage   = 1;
    }

    protected override void UpdateState()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < fleeRange)
            state = AIState.Patrol;
        else if (dist <= attackRange)
            state = AIState.Attack;
        else if (dist <= detectionRange)
            state = AIState.Chase;
        else
            state = AIState.Patrol;
    }

    protected override void Patrol()
    {
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < fleeRange)
            {
                float dir = transform.position.x > player.position.x ? 1f : -1f;
                rb.linearVelocity = new Vector2(moveSpeed * dir, rb.linearVelocity.y);
                return;
            }
        }
        base.Patrol();
    }

    protected override void DealDamage()
    {
        if (arrowPrefab == null || bowPoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, bowPoint.position, Quaternion.identity);
        float dir = (player != null && player.position.x > transform.position.x) ? 1f : -1f;

        // Pasar el collider del arquero para que la flecha lo ignore al spawnear
        Collider2D myCol = GetComponent<Collider2D>();
        arrow.GetComponent<ArrowProjectile>()?.Initialize(new Vector2(dir, 0), attackDamage, myCol);
    }
}

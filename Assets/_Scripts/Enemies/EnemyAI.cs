using UnityEngine;

/// <summary>
/// EnemyAI - IA base para todos los enemigos. Patrullar → Detectar → Perseguir → Atacar
/// BUG FIX: player se busca en Start() no Awake(), y con fallback en Update por si acaso.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] protected float detectionRange  = 10f;
    [SerializeField] protected float attackRange     = 1f;
    [SerializeField] protected LayerMask playerLayer;

    [Header("Movimiento")]
    [SerializeField] protected float moveSpeed       = 2f;
    [SerializeField] protected float patrolDistance  = 3f;

    [Header("Combate")]
    [SerializeField] protected int   attackDamage    = 1;
    [SerializeField] protected float attackCooldown  = 1.5f;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius    = 0.5f;

    protected enum AIState { Patrol, Chase, Attack, Dead }
    protected AIState state = AIState.Patrol;

    protected Rigidbody2D   rb;
    protected EnemyHealth   health;
    protected Animator      animator;
    protected Transform     player;

    protected Vector2 patrolOrigin;
    protected float   patrolDirection = 1f;
    protected float   attackTimer;
    protected bool    facingRight = true;

    // Tiempo sin ver al player antes de volver a patrullar
    private float lostPlayerTimer;
    private const float LOST_PLAYER_GRACE = 2f;

    private   static readonly int AnimSpeed  = Animator.StringToHash("Speed");
    protected static readonly int AnimAttack = Animator.StringToHash("Attack");

    // ── LIFECYCLE ─────────────────────────────────────────────
    protected virtual void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        health   = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        patrolOrigin = transform.position;
        health.OnEnemyDied += OnDied;
    }

    protected virtual void Start()
    {
        // FIX BUG 1: buscar en Start(), no Awake(), para que Skarn ya exista
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (state == AIState.Dead) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        // FIX BUG 1: re-intentar encontrar al player si aún no se encontró
        if (player == null) FindPlayer();

        UpdateState();
        ExecuteState();
        UpdateAnimator();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    // ── MÁQUINA DE ESTADOS ────────────────────────────────────
    protected virtual void UpdateState()
    {
        if (player == null) { state = AIState.Patrol; return; }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            lostPlayerTimer = LOST_PLAYER_GRACE;
            state = AIState.Attack;
        }
        else if (dist <= detectionRange)
        {
            lostPlayerTimer = LOST_PLAYER_GRACE;
            state = AIState.Chase;
        }
        else
        {
            // Grace period: perseguir un poco más antes de volver a patrullar
            if (lostPlayerTimer > 0f)
            {
                lostPlayerTimer -= Time.deltaTime;
                state = AIState.Chase;
            }
            else
            {
                state = AIState.Patrol;
            }
        }
    }

    protected virtual void ExecuteState()
    {
        switch (state)
        {
            case AIState.Patrol: Patrol(); break;
            case AIState.Chase:  Chase();  break;
            case AIState.Attack: Attack(); break;
        }
    }

    // ── COMPORTAMIENTOS ───────────────────────────────────────
    protected virtual void Patrol()
    {
        rb.linearVelocity = new Vector2(moveSpeed * patrolDirection, rb.linearVelocity.y);
        float distFromOrigin = Mathf.Abs(transform.position.x - patrolOrigin.x);
        if (distFromOrigin >= patrolDistance) { patrolDirection *= -1; Flip(); }
    }

    protected virtual void Chase()
    {
        if (player == null) return;
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveSpeed * dir, rb.linearVelocity.y);
        if (dir > 0 && !facingRight) Flip();
        if (dir < 0 &&  facingRight) Flip();
    }

    protected virtual void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            animator?.SetTrigger(AnimAttack);
            DealDamage();
        }
    }

    protected virtual void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint != null ? attackPoint.position : transform.position,
            attackRadius, playerLayer);
        hit?.GetComponent<SkarnHealth>()?.TakeDamage(attackDamage);
    }

    protected virtual void OnDied()
    {
        state = AIState.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s;
    }

    protected virtual void UpdateAnimator()
    {
        animator?.SetFloat(AnimSpeed, Mathf.Abs(rb.linearVelocity.x));
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

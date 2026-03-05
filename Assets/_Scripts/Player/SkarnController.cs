using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// SkarnController - Controla el movimiento y combate del protagonista esqueleto.
/// Bone Crusher: Made of Bones | Unicorn Games
/// 
/// CONTROLES:
///   Flechas Izq/Der  → Moverse
///   Flecha Arriba     → Saltar
///   Barra Espaciadora → Atacar con espada
///   Ctrl              → Lanzar magia
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
public class SkarnController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // PARÁMETROS DE MOVIMIENTO
    // ─────────────────────────────────────────────────────────────
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // ─────────────────────────────────────────────────────────────
    // PARÁMETROS DE COMBATE
    // ─────────────────────────────────────────────────────────────
    [Header("Combate - Espada")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int swordDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Combate - Magia")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform magicSpawnPoint;
    [SerializeField] private float magicCooldown = 1f;
    [SerializeField] private int magicDamage = 2;

    // ─────────────────────────────────────────────────────────────
    // REFERENCIAS PRIVADAS
    // ─────────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator animator;
    private SkarnHealth health;

    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    private float attackTimer;
    private float magicTimer;
    private float attackDuration;  // duracion del clip de ataque

    // Poderes otorgados por aliados (se activan al derrotar ciertos enemigos)
    private bool hasBatPoison = false;      // Murciélago Venenoso - Piso 23
    private bool hasSpecterPhase = false;   // Espectro - Piso 17
    private bool hasGolemShield = false;    // Gólem - Piso 9

    // ─────────────────────────────────────────────────────────────
    // HASHES DE ANIMATOR (más eficiente que strings)
    // ─────────────────────────────────────────────────────────────
    private static readonly int AnimSpeed      = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimAttack     = Animator.StringToHash("Attack");
    private static readonly int AnimMagic      = Animator.StringToHash("Magic");
    private static readonly int AnimHurt       = Animator.StringToHash("Hurt");
    private static readonly int AnimDie        = Animator.StringToHash("Die");

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb        = GetComponent<Rigidbody2D>();
        animator  = GetComponent<Animator>();
        health    = GetComponent<SkarnHealth>();
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;

        HandleInput();
        HandleAttackTimers();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead) return;

        CheckGround();
        Move();
    }

    // ─────────────────────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────────────────────
    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Salto
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            Jump();

        // Ataque con espada (cooldown controla el re-uso, no isAttacking)
        if (Input.GetKeyDown(KeyCode.Space) && attackTimer <= 0f)
            SwordAttack();

        // Magia
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            if (magicTimer <= 0f)
                CastMagic();
    }

    private void HandleAttackTimers()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (magicTimer  > 0f) magicTimer  -= Time.deltaTime;
    }

    // ─────────────────────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────────────────────
    private void Move()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Voltear sprite según dirección
        if (horizontalInput > 0 && !facingRight)  Flip();
        if (horizontalInput < 0 &&  facingRight)  Flip();
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ─────────────────────────────────────────────────────────────
    // COMBATE - ESPADA
    // ─────────────────────────────────────────────────────────────
    private void SwordAttack()
    {
        attackTimer = attackCooldown;
        animator.SetTrigger(AnimAttack);

        // Detectar enemigos en rango
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int dmg = swordDamage;
                if (hasBatPoison) dmg += 1; // Bonus de veneno del murciélago aliado
                enemy.TakeDamage(dmg);
            }
        }
    }

    // Mantenido por compatibilidad con Animation Events opcionales
    public void OnSwordAttackComplete() { }

    // ─────────────────────────────────────────────────────────────
    // COMBATE - MAGIA
    // ─────────────────────────────────────────────────────────────
    private void CastMagic()
    {
        if (magicProjectilePrefab == null) return;

        magicTimer = magicCooldown;
        animator.SetTrigger(AnimMagic);

        GameObject projectile = Instantiate(
            magicProjectilePrefab,
            magicSpawnPoint.position,
            Quaternion.identity
        );

        // FIX BUG 3: pasar el collider de Skarn para que el proyectil lo ignore
        MagicProjectile mp = projectile.GetComponent<MagicProjectile>();
        if (mp != null)
        {
            Collider2D skarnCol = GetComponent<Collider2D>();
            mp.Initialize(facingRight ? Vector2.right : Vector2.left,
                          magicDamage, hasSpecterPhase, skarnCol);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ANIMATOR
    // ─────────────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        animator.SetFloat(AnimSpeed, Mathf.Abs(horizontalInput));
        animator.SetBool(AnimIsGrounded, isGrounded);
    }

    public void TriggerHurtAnimation()  => animator.SetTrigger(AnimHurt);
    public void TriggerDeathAnimation() => animator.SetTrigger(AnimDie);

    // ─────────────────────────────────────────────────────────────
    // PODERES DE ALIADOS
    // ─────────────────────────────────────────────────────────────
    /// <summary>Activa el poder del aliado desbloqueado al derrotarlo.</summary>
    public void UnlockAllyPower(AllyType ally)
    {
        switch (ally)
        {
            case AllyType.VenomBat:
                hasBatPoison = true;
                Debug.Log("¡Murciélago Venenoso se une a Skarn! +1 daño de espada con veneno.");
                break;
            case AllyType.Specter:
                hasSpecterPhase = true;
                Debug.Log("¡Espectro se une a Skarn! La magia ahora atraviesa escudos.");
                break;
            case AllyType.Golem:
                hasGolemShield = true;
                if (health != null) health.ActivateGolemShield();
                Debug.Log("¡Gólem se une a Skarn! Escudo que absorbe 1 golpe extra.");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // GIZMOS DE DEPURACIÓN
    // ─────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

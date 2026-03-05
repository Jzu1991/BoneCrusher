using UnityEngine;

/// <summary>
/// MagicProjectile - Proyectil mágico de Skarn.
/// BUG FIX: usa Physics2D.IgnoreCollision con el collider de Skarn para no
/// autodestruirse al spawnear dentro/cerca del collider del jugador.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MagicProjectile : MonoBehaviour
{
    [SerializeField] private float speed    = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    private Vector2 direction;
    private int     damage;
    private bool    piercesShields;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Inicializar dirección, daño, y opcionalmente ignorar el collider de Skarn.
    /// </summary>
    public void Initialize(Vector2 dir, int dmg, bool spectrePhase = false,
                           Collider2D skarnCollider = null)
    {
        direction      = dir.normalized;
        damage         = dmg;
        piercesShields = spectrePhase;

        // FIX BUG 3: ignorar colisión con el propio collider de Skarn
        if (skarnCollider != null)
        {
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol != null)
                Physics2D.IgnoreCollision(myCol, skarnCollider, true);
        }

        rb.linearVelocity = direction * speed;

        // Voltear sprite si va a la izquierda
        if (dir.x < 0)
        {
            Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar otros proyectiles y triggers
        if (other.isTrigger) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, piercesShields);
            Destroy(gameObject);
            return;
        }

        // Golpear geometría sólida (suelo, paredes)
        Destroy(gameObject);
    }
}

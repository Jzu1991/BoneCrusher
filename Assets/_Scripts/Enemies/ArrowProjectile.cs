using UnityEngine;

/// <summary>Flecha disparada por el Esqueleto Arquero.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed   = 7f;
    [SerializeField] private float lifetime = 3f;
    private int damage;

    public void Initialize(Vector2 dir, int dmg, Collider2D shooterCollider = null)
    {
        damage = dmg;
        var rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
        rb.gravityScale   = 0.3f;
        Destroy(gameObject, lifetime);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Ignorar colision con el collider del arquero que dispara
        if (shooterCollider != null)
        {
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol != null)
                Physics2D.IgnoreCollision(myCol, shooterCollider, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var skarn = other.GetComponent<SkarnHealth>();
        if (skarn != null) { skarn.TakeDamage(damage); Destroy(gameObject); return; }
        if (!other.isTrigger) Destroy(gameObject);
    }
}

using UnityEngine;

/// <summary>DamageZone - Zona que daña a Skarn al pisarla.</summary>
public class DamageZone : MonoBehaviour
{
    [SerializeField] private int   damage         = 1;
    [SerializeField] private float damageCooldown = 1f;
    private float damageTimer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || damageTimer > 0f) return;
        damageTimer = damageCooldown;
        other.GetComponent<SkarnHealth>()?.TakeDamage(damage);
    }

    private void Update() { if (damageTimer > 0f) damageTimer -= Time.deltaTime; }
}

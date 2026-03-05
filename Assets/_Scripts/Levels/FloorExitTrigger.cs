using UnityEngine;

/// <summary>
/// FloorExitTrigger - Activa la transición al piso siguiente.
/// Usa OnTriggerStay2D para que funcione incluso si el jugador ya estaba
/// dentro del portal cuando se derrota al último enemigo del piso.
/// </summary>
public class FloorExitTrigger : MonoBehaviour
{
    [SerializeField] private bool requiresAllEnemiesDefeated = true;

    // Cooldown para no llamar FloorCompleted() cada frame de physics
    private float checkCooldown;
    private const float CHECK_INTERVAL = 0.25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryExit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (checkCooldown > 0f) return;
        TryExit(other);
    }

    private void Update()
    {
        if (checkCooldown > 0f)
            checkCooldown -= Time.deltaTime;
    }

    private void TryExit(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (requiresAllEnemiesDefeated && !AllEnemiesDefeated())
        {
            checkCooldown = CHECK_INTERVAL;
            return;
        }

        GameManager.Instance?.FloorCompleted();
    }

    private bool AllEnemiesDefeated()
    {
        Transform floorRoot = transform.parent;

        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth e in allEnemies)
        {
            // Protección contra objetos destruidos durante la iteración
            if (e == null) continue;
            if (e.IsDead) continue;

            // Si el enemigo es hijo del mismo piso root, hay enemigos vivos
            if (floorRoot != null && e.transform.IsChildOf(floorRoot)) return false;

            // Fallback: si no tiene piso padre, verificar distancia al trigger
            if (floorRoot == null)
            {
                float dist = Vector2.Distance(e.transform.position, transform.position);
                if (dist < 20f) return false;
            }
        }

        return true;
    }
}

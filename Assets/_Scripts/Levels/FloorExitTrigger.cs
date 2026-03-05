using UnityEngine;

/// <summary>
/// FloorExitTrigger - Activa la transición al piso siguiente.
/// BUG FIX: filtra enemigos por floor tag para no bloquear con enemigos de otros pisos.
/// </summary>
public class FloorExitTrigger : MonoBehaviour
{
    [SerializeField] private bool requiresAllEnemiesDefeated = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (requiresAllEnemiesDefeated)
        {
            // FIX BUG 2: solo buscar enemigos en el mismo piso (mismo padre root)
            // El piso de este trigger es su abuelo (Floor_XX -> FloorExit)
            Transform floorRoot = transform.parent;

            EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (EnemyHealth e in allEnemies)
            {
                if (e.IsDead) continue;

                // Si el enemigo es hijo del mismo piso root, bloqueamos
                if (floorRoot != null && e.transform.IsChildOf(floorRoot)) return;

                // Fallback: si no tiene piso padre, verificar distancia al trigger
                if (floorRoot == null)
                {
                    float dist = Vector2.Distance(e.transform.position, transform.position);
                    if (dist < 20f) return;
                }
            }
        }

        GameManager.Instance?.FloorCompleted();
    }
}

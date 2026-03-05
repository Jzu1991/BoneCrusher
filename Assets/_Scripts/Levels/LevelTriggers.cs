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

// ─────────────────────────────────────────────────────────────
/// <summary>CheckpointTrigger - Guarda la posición del jugador.</summary>
public class CheckpointTrigger : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player")) return;
        activated = true;
        GameManager.Instance?.SetCheckpoint(transform);
        Debug.Log($"[Checkpoint] Guardado en {transform.position}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 2, 0));
    }
}

// ─────────────────────────────────────────────────────────────
/// <summary>AdventurerDialogueTrigger - Diálogo pre-combate con el aventurero.</summary>
public class AdventurerDialogueTrigger : MonoBehaviour
{
    [SerializeField] private string adventurerName = "Guerrero Aventurero";
    [SerializeField][TextArea(2,5)] private string dialogue = "¡Detente, monstruo!";
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMPro.TMP_Text dialogueText;
    [SerializeField] private TMPro.TMP_Text speakerName;
    [SerializeField] private float displayDuration = 3f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(ShowDialogue());
    }

    private System.Collections.IEnumerator ShowDialogue()
    {
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (speakerName   != null) speakerName.text  = adventurerName;
        if (dialogueText  != null) dialogueText.text = dialogue;
        yield return new WaitForSecondsRealtime(displayDuration);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

// ─────────────────────────────────────────────────────────────
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

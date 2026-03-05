using UnityEngine;

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

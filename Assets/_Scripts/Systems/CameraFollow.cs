using UnityEngine;

/// <summary>
/// CameraFollow - La camara sigue a Skarn suavemente.
/// Agregar al GameObject "Main Camera".
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private float     smoothSpeed = 5f;
    [SerializeField] private Vector3   offset      = new Vector3(0f, 1f, -10f);

    [Header("Limites (opcional)")]
    [SerializeField] private bool  useLimits  = false;
    [SerializeField] private float minY       = -5f;
    [SerializeField] private float maxY       = 200f;

    private void LateUpdate()
    {
        if (target == null)
        {
            // Buscar a Skarn automaticamente si no esta asignado
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        Vector3 desired  = target.position + offset;
        if (useLimits)
            desired.y = Mathf.Clamp(desired.y, minY, maxY);

        desired.z = offset.z; // Mantener Z fija
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}

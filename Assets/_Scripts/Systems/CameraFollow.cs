using UnityEngine;

/// <summary>
/// CameraFollow - Camara fija por piso completo.
/// En lugar de seguir a Skarn, apunta al centro del piso actual
/// y ajusta el ortographic size para mostrar el piso entero.
/// Se actualiza al cambiar de piso via SetFloor().
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    // Cada piso mide 22u de ancho x ~10u de alto.
    // El piso N tiene su origen en Y = (30 - N) * 12
    // Centro visual del piso: X=0, Y=origen+4 (suelo en Y=-1, techo ~Y=9)
    private const float FLOOR_HEIGHT  = 12f;
    private const float FLOOR_CENTER_Y_OFFSET = 4f;   // desde el origen del piso
    private const float FLOOR_WIDTH   = 22f;
    private const float FLOOR_VIS_H   = 10f;          // altura visible del piso

    [Header("Configuracion")]
    [SerializeField] private float transitionSpeed = 4f;  // suavidad al cambiar piso

    private Camera      cam;
    private Vector3     targetPos;
    private float       targetOrthoSize;
    private int         currentFloor = 30;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        GoToFloor(30, instant: true);
    }

    private void Start()
    {
        // Sincronizar con GameManager al iniciar
        if (GameManager.Instance != null)
            GoToFloor(GameManager.Instance.CurrentFloor, instant: true);
    }

    private void LateUpdate()
    {
        // Interpolar suavemente hacia el piso objetivo
        transform.position = Vector3.Lerp(
            transform.position, targetPos,
            transitionSpeed * Time.deltaTime);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize, targetOrthoSize,
            transitionSpeed * Time.deltaTime);
    }

    /// <summary>Llamar desde GameManager al completar un piso.</summary>
    public void SetFloor(int floorNumber, bool instant = false)
    {
        if (currentFloor == floorNumber && !instant) return;
        currentFloor = floorNumber;
        GoToFloor(floorNumber, instant);
    }

    private void GoToFloor(int floorNumber, bool instant)
    {
        float floorOriginY = (30 - floorNumber) * FLOOR_HEIGHT;
        float centerX = 0f;
        float centerY = floorOriginY + FLOOR_CENTER_Y_OFFSET;

        // Orthographic size que muestra el piso completo
        // A 16:9: ancho = orthoSize * 2 * (16/9)
        // Para ancho=22u: orthoSize = 22 / (2 * 16/9) = 22*9/32 = 6.1875
        float aspect    = cam != null ? cam.aspect : (16f / 9f);
        float sizeByW   = (FLOOR_WIDTH  / 2f) / aspect;
        float sizeByH   = FLOOR_VIS_H / 2f;
        // Usar el mayor para que quepan ambas dimensiones
        float orthoSize = Mathf.Max(sizeByW, sizeByH) + 0.3f; // +0.3 de margen

        targetPos       = new Vector3(centerX, centerY, -10f);
        targetOrthoSize = orthoSize;

        if (instant || cam == null)
        {
            transform.position   = targetPos;
            if (cam != null) cam.orthographicSize = targetOrthoSize;
        }
    }

    // Referencia no usada pero mantenida para compatibilidad con el Setup
    [SerializeField] private Transform target;
    [SerializeField] private Vector3   offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float     smoothSpeed = 5f;
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Núcleo del loop de juego para la demo Pisos 30-25.
/// BUG FIX: TransitionToNextFloor ahora teletransporta a Skarn al inicio del piso siguiente.
/// BUG FIX: FloorExitTrigger usa un floor tag para filtrar enemigos solo del piso actual.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Piso Actual")]
    [SerializeField] private int  currentFloor = 30;
    [SerializeField] private int  targetFloor  = 25;
    [SerializeField] private bool floorCleared = false;

    [Header("Referencias")]
    [SerializeField] private SkarnHealth skarnHealth;
    [SerializeField] private HUDManager  hud;
    [SerializeField] private GameObject  gameOverPanel;
    [SerializeField] private GameObject  victoryPanel;

    [Header("Revida")]
    [SerializeField] private float     reviveDelay = 2f;
    [SerializeField] private Transform checkpointPosition;

    // Los pisos están apilados cada 12 unidades en Y. Piso 30 = Y:0, 29 = Y:12, etc.
    // Skarn empieza en piso 30 en x=-7, y=1. Al avanzar sube 12u en Y.
    private const float FLOOR_HEIGHT   = 12f;
    private const float PLAYER_START_X = -7f;
    private const float PLAYER_START_Y =  1f;

    private bool gameIsOver = false;

    // ── LIFECYCLE ─────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (skarnHealth == null)
            skarnHealth = FindFirstObjectByType<SkarnHealth>();

        if (skarnHealth != null)
        {
            skarnHealth.OnSkarnDied      += OnSkarnDied;
            skarnHealth.OnGameOver       += OnGameOver;
            skarnHealth.OnSkullsChanged  += (cur, max) => hud?.UpdateSkulls(cur, max);
            skarnHealth.OnCreditsChanged += (c)        => hud?.UpdateCredits(c);
        }

        hud?.SetFloor(currentFloor);
        hud?.UpdateSkulls(SkarnHealth.BASE_SKULLS, SkarnHealth.BASE_SKULLS);
        hud?.UpdateCredits(SkarnHealth.MAX_CREDITS);
        Debug.Log($"[GameManager] Demo iniciada. Piso {currentFloor} → {targetFloor}");
    }

    // ── GESTIÓN DE PISOS ──────────────────────────────────────
    public void FloorCompleted()
    {
        if (floorCleared) return;
        floorCleared = true;

        skarnHealth?.TryIncreaseMaxSkulls();

        // Victoria: completar el ultimo piso objetivo (25)
        if (currentFloor == targetFloor)
        {
            hud?.SetFloor(currentFloor);
            StartCoroutine(ShowVictory());
            return;
        }

        currentFloor--;
        hud?.SetFloor(currentFloor);
        StartCoroutine(TransitionToNextFloor());
    }

    private IEnumerator TransitionToNextFloor()
    {
        yield return new WaitForSeconds(0.5f);

        // FIX BUG 2: mover a Skarn al inicio del nuevo piso
        // Piso 30=índice 0 (Y=0), 29=índice 1 (Y=12), etc.
        int floorIndex = 30 - currentFloor;           // cuántos pisos subió
        float newY = PLAYER_START_Y + floorIndex * FLOOR_HEIGHT;

        SkarnController skarn = FindFirstObjectByType<SkarnController>();
        if (skarn != null)
        {
            // Teletransportar desactivando física un frame para evitar glitches
            Rigidbody2D srb = skarn.GetComponent<Rigidbody2D>();
            srb.linearVelocity = Vector2.zero;
            skarn.transform.position = new Vector3(PLAYER_START_X, newY, 0f);
        }

        floorCleared = false;

        if (currentFloor == 26)
        {
            PuzzleManager pm = FindFirstObjectByType<PuzzleManager>();
            pm?.ActivatePuzzle(1);
        }

        Debug.Log($"[GameManager] Avanzó al piso {currentFloor}. Skarn en Y={newY}");
    }

    // ── MUERTE Y REVIDA ───────────────────────────────────────
    private void OnSkarnDied()
    {
        if (gameIsOver) return;
        StartCoroutine(ReviveAfterDelay());
    }

    private IEnumerator ReviveAfterDelay()
    {
        yield return new WaitForSeconds(reviveDelay);
        if (skarnHealth.Credits >= 0)
        {
            if (checkpointPosition != null)
            {
                SkarnController skarn = FindFirstObjectByType<SkarnController>();
                if (skarn != null)
                {
                    skarn.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                    skarn.transform.position = checkpointPosition.position;
                }
            }
            skarnHealth.Revive();
        }
    }

    private void OnGameOver()
    {
        gameIsOver = true;
        StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(1.5f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private IEnumerator ShowVictory()
    {
        yield return new WaitForSeconds(1f);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Debug.Log("[GameManager] ¡Demo completada! Pisos 30-25 superados.");
    }

    // ── CHECKPOINTS ───────────────────────────────────────────
    public void SetCheckpoint(Transform position) => checkpointPosition = position;

    // ── UI ────────────────────────────────────────────────────
    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void QuitGame()    => Application.Quit();
    public int CurrentFloor   => currentFloor;
}

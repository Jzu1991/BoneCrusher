using System;
using UnityEngine;

/// <summary>
/// SkarnHealth - Sistema de vida con cráneos y créditos.
/// 
/// REGLAS (según High Concept):
///   - 3 cráneos base (vida), máx 5 si se acumulan sin perder
///   - Al derrotar aventureros, recupera 1 cráneo
///   - 5 créditos (revidas) antes de reiniciar desde cero
///   - Escudo de Gólem: absorbe 1 golpe extra
/// </summary>
public class SkarnHealth : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // CONSTANTES
    // ─────────────────────────────────────────────────────────────
    public const int BASE_SKULLS   = 3;
    public const int MAX_SKULLS    = 5;
    public const int MAX_CREDITS   = 5;

    // ─────────────────────────────────────────────────────────────
    // ESTADO
    // ─────────────────────────────────────────────────────────────
    [Header("Estado de Vida")]
    [SerializeField] private int currentSkulls  = BASE_SKULLS;
    [SerializeField] private int maxSkulls      = BASE_SKULLS;
    [SerializeField] private int credits        = MAX_CREDITS;

    private bool isDead          = false;
    private bool golemShieldActive = false;
    private bool isInvincible    = false;   // invencibilidad temporal tras recibir daño

    [Header("Invencibilidad")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer;

    // ─────────────────────────────────────────────────────────────
    // EVENTOS
    // ─────────────────────────────────────────────────────────────
    public event Action<int, int> OnSkullsChanged;  // (actual, máximo)
    public event Action<int>      OnCreditsChanged; // (créditos restantes)
    public event Action           OnSkarnDied;
    public event Action           OnSkarnRevived;
    public event Action           OnGameOver;

    // ─────────────────────────────────────────────────────────────
    // PROPIEDADES
    // ─────────────────────────────────────────────────────────────
    public bool IsDead       => isDead;
    public int  CurrentSkulls => currentSkulls;
    public int  MaxSkulls     => maxSkulls;
    public int  Credits       => credits;

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        currentSkulls = BASE_SKULLS;
        maxSkulls     = BASE_SKULLS;
        credits       = MAX_CREDITS;
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                isInvincible = false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // RECIBIR DAÑO
    // ─────────────────────────────────────────────────────────────
    public void TakeDamage(int amount = 1)
    {
        if (isDead || isInvincible) return;

        // Escudo del Gólem absorbe el primer golpe
        if (golemShieldActive)
        {
            golemShieldActive = false;
            Debug.Log("¡Escudo de Gólem absorbió el golpe!");
            StartInvincibility();
            return;
        }

        currentSkulls -= amount;
        currentSkulls  = Mathf.Max(currentSkulls, 0);

        GetComponent<SkarnController>()?.TriggerHurtAnimation();
        OnSkullsChanged?.Invoke(currentSkulls, maxSkulls);

        if (currentSkulls <= 0)
            Die();
        else
            StartInvincibility();
    }

    // ─────────────────────────────────────────────────────────────
    // RECUPERAR CRÁNEO (al derrotar aventureros)
    // ─────────────────────────────────────────────────────────────
    public void HealOnAdventurerKill()
    {
        if (currentSkulls < maxSkulls)
        {
            currentSkulls++;
            OnSkullsChanged?.Invoke(currentSkulls, maxSkulls);
            Debug.Log($"¡Aventurero derrotado! Cráneo recuperado: {currentSkulls}/{maxSkulls}");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ACUMULAR CRÁNEOS (subir máximo hasta 5)
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Llamar cuando Skarn completa un piso sin perder cráneos.
    /// Incrementa el máximo hasta MAX_SKULLS = 5.
    /// </summary>
    public void TryIncreaseMaxSkulls()
    {
        if (maxSkulls < MAX_SKULLS)
        {
            maxSkulls++;
            currentSkulls = Mathf.Min(currentSkulls + 1, maxSkulls);
            OnSkullsChanged?.Invoke(currentSkulls, maxSkulls);
            Debug.Log($"¡Máximo de cráneos aumentado a {maxSkulls}!");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // MUERTE Y REVIDA
    // ─────────────────────────────────────────────────────────────
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        GetComponent<SkarnController>()?.TriggerDeathAnimation();
        OnSkarnDied?.Invoke();

        if (credits > 0)
        {
            credits--;
            OnCreditsChanged?.Invoke(credits);
            // El GameManager escucha OnSkarnDied y llama Revive() tras la animación
        }
        else
        {
            OnGameOver?.Invoke();
        }
    }

    /// <summary>Revivir a Skarn con cráneos base en el último checkpoint.</summary>
    public void Revive()
    {
        isDead        = false;
        currentSkulls = BASE_SKULLS;
        isInvincible  = false;
        OnSkarnRevived?.Invoke();
        OnSkullsChanged?.Invoke(currentSkulls, maxSkulls);
        Debug.Log($"Skarn revivió. Créditos restantes: {credits}");
    }

    // ─────────────────────────────────────────────────────────────
    // PODER DEL GÓLEM
    // ─────────────────────────────────────────────────────────────
    public void ActivateGolemShield()
    {
        golemShieldActive = true;
        Debug.Log("¡Escudo de Gólem activado!");
    }

    // ─────────────────────────────────────────────────────────────
    // UTILIDADES
    // ─────────────────────────────────────────────────────────────
    private void StartInvincibility()
    {
        isInvincible      = true;
        invincibilityTimer = invincibilityDuration;
    }
}

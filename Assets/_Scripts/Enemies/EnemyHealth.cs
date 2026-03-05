using System;
using UnityEngine;

/// <summary>
/// EnemyHealth - Sistema de vida genérico para todos los enemigos.
/// La vida base es de 5 impactos.
/// Los aventureros tienen vida en incrementos de 5 según dificultad del piso.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] protected int maxHealth    = 5;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected bool hasShield   = false; // Solo algunos enemigos elite

    [Header("Tipo")]
    [SerializeField] public EnemyType enemyType = EnemyType.Monster;
    [SerializeField] public bool isAlly         = false; // ¿Puede unirse a Skarn?
    [SerializeField] public AllyType allyType;

    public event Action<int, int> OnHealthChanged; // (actual, máximo)
    public event Action           OnEnemyDied;

    public bool IsDead      => currentHealth <= 0;
    public int  CurrentHP   => currentHealth;
    public int  MaxHP       => maxHealth;

    protected Animator animator;
    private static readonly int AnimHurt = Animator.StringToHash("Hurt");
    private static readonly int AnimDie  = Animator.StringToHash("Die");

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    /// <summary>Recibir daño. piercesShields = poder del Espectro.</summary>
    public virtual void TakeDamage(int amount, bool piercesShields = false)
    {
        if (IsDead) return;

        if (hasShield && !piercesShields)
        {
            hasShield = false;
            Debug.Log($"{gameObject.name} escudo destruido.");
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        animator?.SetTrigger(AnimHurt);

        if (IsDead)
            Die();
    }

    protected virtual void Die()
    {
        animator?.SetTrigger(AnimDie);
        OnEnemyDied?.Invoke();

        // Si es un aliado, notificar al jugador para desbloquear poder
        if (isAlly)
        {
            SkarnController skarn = FindFirstObjectByType<SkarnController>();
            skarn?.UnlockAllyPower(allyType);
        }

        // Si es un aventurero, sanar a Skarn
        if (enemyType == EnemyType.Adventurer)
        {
            SkarnHealth skarnHealth = FindFirstObjectByType<SkarnHealth>();
            skarnHealth?.HealOnAdventurerKill();
        }

        // Destruir tras la animación de muerte
        Destroy(gameObject, 1.5f);
    }
}

public enum EnemyType { Monster, Adventurer, Boss }

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUDManager - Interfaz de usuario durante el juego.
/// Muestra: Cráneos de vida, créditos (revidas) y número de piso actual.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Cráneos (Vida)")]
    [SerializeField] private Image[] skullIcons;     // Array de 5 iconos de cráneo
    [SerializeField] private Sprite  skullFull;      // Sprite cráneo lleno
    [SerializeField] private Sprite  skullEmpty;     // Sprite cráneo vacío
    [SerializeField] private Sprite  skullLocked;    // Sprite cráneo bloqueado (>máx)

    [Header("Créditos")]
    [SerializeField] private TMP_Text creditsText;   // "x5"
    [SerializeField] private Image    creditIcon;    // Icono del cráneo de crédito

    [Header("Piso")]
    [SerializeField] private TMP_Text floorText;     // "PISO 30"

    [Header("Aliados")]
    [SerializeField] private GameObject[] allyIcons; // Iconos de aliados desbloqueados

    // ─────────────────────────────────────────────────────────────
    // ACTUALIZAR CRÁNEOS
    // ─────────────────────────────────────────────────────────────
    /// <summary>Actualiza los iconos de cráneo según vida actual y máxima.</summary>
    public void UpdateSkulls(int current, int max)
    {
        for (int i = 0; i < skullIcons.Length; i++)
        {
            if (skullIcons[i] == null) continue;

            if (i >= max)
            {
                // Cráneo bloqueado (no alcanzado aún)
                skullIcons[i].sprite = skullLocked;
                skullIcons[i].color  = new Color(1, 1, 1, 0.3f);
            }
            else if (i < current)
            {
                // Cráneo lleno
                skullIcons[i].sprite = skullFull;
                skullIcons[i].color  = Color.white;
            }
            else
            {
                // Cráneo vacío
                skullIcons[i].sprite = skullEmpty;
                skullIcons[i].color  = new Color(1, 1, 1, 0.6f);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ACTUALIZAR CRÉDITOS
    // ─────────────────────────────────────────────────────────────
    public void UpdateCredits(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"x{credits}";
    }

    // ─────────────────────────────────────────────────────────────
    // ACTUALIZAR PISO
    // ─────────────────────────────────────────────────────────────
    public void SetFloor(int floor)
    {
        if (floorText != null)
            floorText.text = $"PISO {floor}";
    }

    // ─────────────────────────────────────────────────────────────
    // MOSTRAR ALIADO DESBLOQUEADO
    // ─────────────────────────────────────────────────────────────
    public void ShowAllyUnlocked(int allyIndex)
    {
        if (allyIndex < allyIcons.Length && allyIcons[allyIndex] != null)
            allyIcons[allyIndex].SetActive(true);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AdventurerDialogueTrigger - Dialogo pre-combate con el aventurero.
/// Crea su propio panel de UI en runtime, no necesita referencias del Inspector.
/// </summary>
public class AdventurerDialogueTrigger : MonoBehaviour
{
    [SerializeField] private string adventurerName = "Aventurero";
    [SerializeField][TextArea(2,5)] private string dialogue = "Detente!";
    [SerializeField] private float displayDuration = 3f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(ShowDialogue());
    }

    private IEnumerator ShowDialogue()
    {
        // Pausar el juego (WaitForSecondsRealtime ignora timeScale)
        Time.timeScale = 0f;

        // Construir panel de UI en runtime
        GameObject panel = BuildDialoguePanel();

        yield return new WaitForSecondsRealtime(displayDuration);

        Destroy(panel);
        Time.timeScale = 1f;
    }

    private GameObject BuildDialoguePanel()
    {
        // Canvas propio (ScreenSpaceOverlay, por encima de todo)
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)canvasGO.GetComponent<CanvasScaler>()).referenceResolution =
            new Vector2(1920, 1080);

        // Panel fondo en la parte inferior de pantalla
        var panelGO   = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0f,   0f);
        panelRect.anchorMax        = new Vector2(1f,   0.28f);
        panelRect.offsetMin        = Vector2.zero;
        panelRect.offsetMax        = Vector2.zero;
        var panelImg  = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.03f, 0.10f, 0.92f);

        // Borde superior sutil
        var borderGO   = new GameObject("Border");
        borderGO.transform.SetParent(panelGO.transform, false);
        var borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin  = new Vector2(0f, 1f);
        borderRect.anchorMax  = new Vector2(1f, 1f);
        borderRect.sizeDelta  = new Vector2(0f, 3f);
        borderRect.anchoredPosition = Vector2.zero;
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(0.5f, 0.2f, 0.8f, 1f);

        // Nombre del hablante
        var nameGO   = new GameObject("SpeakerName");
        nameGO.transform.SetParent(panelGO.transform, false);
        var nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin        = new Vector2(0.05f, 0.65f);
        nameRect.anchorMax        = new Vector2(0.6f,  0.95f);
        nameRect.offsetMin        = Vector2.zero;
        nameRect.offsetMax        = Vector2.zero;
        var nameTMP  = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text      = adventurerName;
        nameTMP.fontSize  = 32;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color     = new Color(1f, 0.85f, 0.3f);

        // Texto del dialogo
        var textGO   = new GameObject("DialogueText");
        textGO.transform.SetParent(panelGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin        = new Vector2(0.05f, 0.05f);
        textRect.anchorMax        = new Vector2(0.95f, 0.62f);
        textRect.offsetMin        = Vector2.zero;
        textRect.offsetMax        = Vector2.zero;
        var textTMP  = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text     = dialogue;
        textTMP.fontSize = 26;
        textTMP.color    = Color.white;

        // Indicador "continua..." en esquina inferior derecha
        var hintGO   = new GameObject("Hint");
        hintGO.transform.SetParent(panelGO.transform, false);
        var hintRect = hintGO.AddComponent<RectTransform>();
        hintRect.anchorMin        = new Vector2(0.75f, 0.02f);
        hintRect.anchorMax        = new Vector2(0.98f, 0.25f);
        hintRect.offsetMin        = Vector2.zero;
        hintRect.offsetMax        = Vector2.zero;
        var hintTMP  = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "[ continua... ]";
        hintTMP.fontSize  = 18;
        hintTMP.fontStyle = FontStyles.Italic;
        hintTMP.color     = new Color(0.6f, 0.5f, 0.8f, 1f);
        hintTMP.alignment = TextAlignmentOptions.BottomRight;

        return canvasGO;
    }
}

using UnityEngine;

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

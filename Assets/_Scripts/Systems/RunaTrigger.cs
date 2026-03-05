using UnityEngine;

/// <summary>RunaTrigger - Trigger individual de runa para el puzzle del Piso 26.</summary>
public class RunaTrigger : MonoBehaviour
{
    private int           runaIndex;
    private PuzzleManager manager;
    private Color         baseColor;
    private SpriteRenderer sr;
    private bool cooldown;

    public void Init(int i, PuzzleManager mgr, Color col)
    {
        runaIndex = i; manager = mgr; baseColor = col;
        var b = transform.Find("Base");
        if (b != null) sr = b.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (cooldown || !other.CompareTag("Player")) return;
        cooldown = true;
        manager.RunaActivated(runaIndex);
        Invoke(nameof(ResetCooldown), 1f);
    }
    void ResetCooldown() => cooldown = false;

    public void Flash(bool on)      { if (sr) sr.color = on ? Color.white : baseColor; }
    public void SetColor(Color c)   { if (sr) sr.color = c; }
    public void RestoreColor()      { if (sr) sr.color = baseColor; }
}

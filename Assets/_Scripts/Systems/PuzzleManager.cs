using System.Collections;
using UnityEngine;

/// <summary>
/// PuzzleManager v3 - Piso 26: runas de presión en el mundo.
/// FIX: al resolver llama GameManager.FloorCompleted() directamente
/// en lugar de depender de que Skarn cruce un trigger bloqueado.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] public GameObject exitDoor;
    [SerializeField] public float displayDelay = 0.6f;

    private RunaTrigger[] runas;
    private int  currentStep = 0;
    private bool solved      = false;
    private bool showing     = false;

    private static readonly Color[] RUNA_COLORS = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.2f, 0.9f, 0.3f),
        new Color(0.2f, 0.4f, 0.9f),
    };
    private static readonly Vector3[] RUNA_POSITIONS = {
        new Vector3(-6f, 0f, 0f),
        new Vector3( 0f, 0f, 0f),
        new Vector3( 6f, 0f, 0f),
    };

    private void Start()
    {
        // Puerta bloqueada al inicio
        SetDoorLocked(true);

        runas = new RunaTrigger[RUNA_COLORS.Length];
        for (int i = 0; i < RUNA_COLORS.Length; i++)
        {
            var go = CreateRunaGO(i);
            runas[i] = go.AddComponent<RunaTrigger>();
            runas[i].Init(i, this, RUNA_COLORS[i]);
        }
        StartCoroutine(ShowHint());
    }

    // ── PUERTA ────────────────────────────────────────────────
    void SetDoorLocked(bool locked)
    {
        if (exitDoor == null) return;
        var col = exitDoor.GetComponent<Collider2D>();
        if (col != null) col.enabled = !locked;

        // NO agregar FloorExitTrigger: PuzzleManager llama FloorCompleted() directamente
        // Agregar FloorExitTrigger causaria una segunda llamada al cruzar el portal

        var sr = exitDoor.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = locked
            ? new Color(0.6f, 0.1f, 0.1f)
            : Color.white;
    }

    // ── PISTA VISUAL ──────────────────────────────────────────
    IEnumerator ShowHint()
    {
        showing = true;
        yield return new WaitForSeconds(1f);
        for (int rep = 0; rep < 2; rep++)
        {
            for (int i = 0; i < runas.Length; i++)
            {
                runas[i].Flash(true);
                yield return new WaitForSeconds(displayDelay);
                runas[i].Flash(false);
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(0.4f);
        }
        showing = false;
    }

    // ── LÓGICA ────────────────────────────────────────────────
    public void RunaActivated(int index)
    {
        if (solved || showing) return;

        if (index == currentStep)
        {
            runas[index].Flash(true);
            currentStep++;
            if (currentStep >= runas.Length)
                StartCoroutine(SolvePuzzle());
        }
        else
        {
            StartCoroutine(FailSequence());
        }
    }

    IEnumerator SolvePuzzle()
    {
        solved = true;
        for (int i = 0; i < 3; i++)
        {
            foreach (var r in runas) r.SetColor(Color.green);
            yield return new WaitForSeconds(0.2f);
            foreach (var r in runas) r.RestoreColor();
            yield return new WaitForSeconds(0.2f);
        }
        SetDoorLocked(false);

        // FIX: llamar FloorCompleted directamente — no esperar trigger de puerta
        yield return new WaitForSeconds(0.8f);
        GameManager.Instance?.FloorCompleted();
    }

    IEnumerator FailSequence()
    {
        showing = true;
        currentStep = 0;
        for (int i = 0; i < 2; i++)
        {
            foreach (var r in runas) r.SetColor(Color.red);
            yield return new WaitForSeconds(0.2f);
            foreach (var r in runas) r.RestoreColor();
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.3f);
        showing = false;
        yield return ShowHint();
    }

    public void ActivatePuzzle(int difficulty)
    {
        currentStep = 0;
        solved      = false;
        StartCoroutine(ShowHint());
    }

    // ── CREAR RUNA ────────────────────────────────────────────
    GameObject CreateRunaGO(int index)
    {
        var go = new GameObject($"Runa_{index}");
        go.transform.SetParent(transform);
        go.transform.localPosition = RUNA_POSITIONS[index];

        // Etiqueta de número
        var numGO = new GameObject("Label");
        numGO.transform.SetParent(go.transform);
        numGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        numGO.transform.localScale    = new Vector3(0.4f, 0.4f, 1f);
        var tmp = numGO.AddComponent<TMPro.TextMeshPro>();
        tmp.text = (index + 1).ToString();
        tmp.fontSize = 8; tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;

        // Plataforma visual
        var baseGO = new GameObject("Base");
        baseGO.transform.SetParent(go.transform);
        baseGO.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        baseGO.transform.localScale    = new Vector3(1.5f, 0.18f, 1f);
        var sr = baseGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        // Sprite blanco procedural (sin dependencia de AssetDatabase)
        var tex = new Texture2D(1,1); tex.SetPixel(0,0,Color.white); tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        sr.color = RUNA_COLORS[index];

        // Trigger
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size   = new Vector2(1.5f, 1.5f);
        col.offset = new Vector2(0f, 0.5f);

        return go;
    }
}

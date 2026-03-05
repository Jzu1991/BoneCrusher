using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUDSetup - Genera el Canvas del HUD en runtime con vida y créditos
/// posicionados arriba a la izquierda. Se auto-enlaza con HUDManager.
/// Adjuntar al mismo GameObject que HUDManager o a uno vacío en la escena.
/// </summary>
[RequireComponent(typeof(HUDManager))]
public class HUDSetup : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private Color skullFullColor   = new Color(0.9f, 0.85f, 0.7f);  // Hueso claro
    [SerializeField] private Color skullEmptyColor  = new Color(0.3f, 0.3f, 0.3f);   // Gris oscuro
    [SerializeField] private Color skullLockedColor = new Color(0.15f, 0.15f, 0.15f); // Casi negro
    [SerializeField] private Color creditColor      = new Color(0.2f, 0.8f, 1f);      // Azul celeste
    [SerializeField] private Color floorColor       = new Color(1f, 0.85f, 0.3f);     // Dorado
    [SerializeField] private Color panelBgColor     = new Color(0f, 0f, 0f, 0.5f);    // Fondo semitransparente

    [Header("Tamaño")]
    [SerializeField] private float skullSize   = 40f;
    [SerializeField] private float skullGap    = 8f;
    [SerializeField] private float padding     = 12f;
    [SerializeField] private float fontSize    = 22f;

    private HUDManager hudManager;

    private void Awake()
    {
        hudManager = GetComponent<HUDManager>();
        BuildHUD();
    }

    private void BuildHUD()
    {
        // ── CANVAS ──────────────────────────────────────────────
        GameObject canvasGO = new GameObject("HUD_Canvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── PANEL CONTENEDOR (arriba izquierda) ─────────────────
        GameObject panelGO = CreateUIElement("HUD_Panel", canvasGO.transform);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot     = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(padding, -padding);

        // Fondo semitransparente
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = panelBgColor;

        // Layout vertical
        VerticalLayoutGroup vLayout = panelGO.AddComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(
            (int)padding, (int)padding, (int)padding, (int)padding);
        vLayout.spacing = 8f;
        vLayout.childAlignment = TextAnchor.UpperLeft;
        vLayout.childForceExpandWidth  = false;
        vLayout.childForceExpandHeight = false;
        vLayout.childControlWidth  = true;
        vLayout.childControlHeight = true;

        ContentSizeFitter panelFitter = panelGO.AddComponent<ContentSizeFitter>();
        panelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        panelFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // ── FILA DE CRÁNEOS (VIDA) ──────────────────────────────
        GameObject skullsRowGO = CreateUIElement("Skulls_Row", panelGO.transform);
        HorizontalLayoutGroup hLayout = skullsRowGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = skullGap;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childForceExpandWidth  = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth  = false;
        hLayout.childControlHeight = false;

        // Label "VIDA"
        GameObject vidaLabel = CreateUIElement("Vida_Label", skullsRowGO.transform);
        TMP_Text vidaTMP = vidaLabel.AddComponent<TextMeshProUGUI>();
        vidaTMP.text = "VIDA";
        vidaTMP.fontSize = fontSize * 0.7f;
        vidaTMP.color = skullFullColor;
        vidaTMP.fontStyle = FontStyles.Bold;
        vidaTMP.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement vidaLE = vidaLabel.AddComponent<LayoutElement>();
        vidaLE.preferredWidth  = 60f;
        vidaLE.preferredHeight = skullSize;

        // Generar sprites procedurales
        Sprite fullSprite   = CreateCircleSprite(skullFullColor);
        Sprite emptySprite  = CreateCircleSprite(skullEmptyColor);
        Sprite lockedSprite = CreateCircleSprite(skullLockedColor);

        // 5 iconos de cráneo
        Image[] skullIcons = new Image[SkarnHealth.MAX_SKULLS];
        for (int i = 0; i < SkarnHealth.MAX_SKULLS; i++)
        {
            GameObject skullGO = CreateUIElement($"Skull_{i}", skullsRowGO.transform);
            Image skullImg = skullGO.AddComponent<Image>();
            skullImg.sprite = (i < SkarnHealth.BASE_SKULLS) ? fullSprite : lockedSprite;
            skullImg.color  = (i < SkarnHealth.BASE_SKULLS) ? Color.white : new Color(1, 1, 1, 0.3f);

            RectTransform skullRect = skullGO.GetComponent<RectTransform>();
            skullRect.sizeDelta = new Vector2(skullSize, skullSize);

            skullIcons[i] = skullImg;
        }

        // ── FILA DE CRÉDITOS ────────────────────────────────────
        GameObject creditsRowGO = CreateUIElement("Credits_Row", panelGO.transform);
        HorizontalLayoutGroup creditsLayout = creditsRowGO.AddComponent<HorizontalLayoutGroup>();
        creditsLayout.spacing = 6f;
        creditsLayout.childAlignment = TextAnchor.MiddleLeft;
        creditsLayout.childForceExpandWidth  = false;
        creditsLayout.childForceExpandHeight = false;
        creditsLayout.childControlWidth  = false;
        creditsLayout.childControlHeight = false;

        // Icono de crédito
        GameObject creditIconGO = CreateUIElement("Credit_Icon", creditsRowGO.transform);
        Image creditImg = creditIconGO.AddComponent<Image>();
        creditImg.sprite = CreateDiamondSprite(creditColor);
        creditImg.color  = Color.white;
        RectTransform creditIconRect = creditIconGO.GetComponent<RectTransform>();
        creditIconRect.sizeDelta = new Vector2(skullSize * 0.7f, skullSize * 0.7f);

        // Texto de créditos
        GameObject creditsTextGO = CreateUIElement("Credits_Text", creditsRowGO.transform);
        TMP_Text creditsTMP = creditsTextGO.AddComponent<TextMeshProUGUI>();
        creditsTMP.text = $"x{SkarnHealth.MAX_CREDITS}";
        creditsTMP.fontSize = fontSize;
        creditsTMP.color = creditColor;
        creditsTMP.fontStyle = FontStyles.Bold;
        creditsTMP.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement creditsLE = creditsTextGO.AddComponent<LayoutElement>();
        creditsLE.preferredWidth  = 80f;
        creditsLE.preferredHeight = skullSize * 0.8f;

        // ── TEXTO DE PISO ───────────────────────────────────────
        GameObject floorGO = CreateUIElement("Floor_Text", panelGO.transform);
        TMP_Text floorTMP = floorGO.AddComponent<TextMeshProUGUI>();
        floorTMP.text = "PISO 30";
        floorTMP.fontSize = fontSize;
        floorTMP.color = floorColor;
        floorTMP.fontStyle = FontStyles.Bold;
        floorTMP.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement floorLE = floorGO.AddComponent<LayoutElement>();
        floorLE.preferredWidth  = 200f;
        floorLE.preferredHeight = skullSize * 0.8f;

        // ── ASIGNAR REFERENCIAS A HUDManager ────────────────────
        SetHUDManagerReferences(skullIcons, fullSprite, emptySprite, lockedSprite,
                                creditsTMP, creditImg, floorTMP);
    }

    private void SetHUDManagerReferences(Image[] skulls, Sprite full, Sprite empty, Sprite locked,
                                          TMP_Text credits, Image creditIcon, TMP_Text floor)
    {
        // Usamos reflection para asignar los campos SerializeField del HUDManager
        var type = typeof(HUDManager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        type.GetField("skullIcons",  flags)?.SetValue(hudManager, skulls);
        type.GetField("skullFull",   flags)?.SetValue(hudManager, full);
        type.GetField("skullEmpty",  flags)?.SetValue(hudManager, empty);
        type.GetField("skullLocked", flags)?.SetValue(hudManager, locked);
        type.GetField("creditsText", flags)?.SetValue(hudManager, credits);
        type.GetField("creditIcon",  flags)?.SetValue(hudManager, creditIcon);
        type.GetField("floorText",   flags)?.SetValue(hudManager, floor);
    }

    // ── UTILIDADES ──────────────────────────────────────────────
    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private Sprite CreateCircleSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    tex.SetPixel(x, y, color);
                else if (dist <= radius + 1f)
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f - (dist - radius)));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateDiamondSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                if (dist <= center - 2f)
                    tex.SetPixel(x, y, color);
                else if (dist <= center)
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f - (dist - (center - 2f)) / 2f));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}

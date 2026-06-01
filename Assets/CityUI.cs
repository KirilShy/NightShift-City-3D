// CityUI.cs — self-building HUD, no manual Unity Editor setup required.
//
// HOW TO USE:
//   Attach this script to any GameObject (e.g. CityManager).
//   Press Play — the Canvas, dark panel, and text appear automatically.
//
// REQUIRES: TextMeshPro (built into Unity 6 — import TMP Essentials once if prompted).

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityUI : MonoBehaviour
{
    private TextMeshProUGUI _statsText;

    // -------------------------------------------------------------------------
    // Awake — build the UI hierarchy before the first frame.
    // -------------------------------------------------------------------------

    void Awake()
    {
        BuildHUD();
    }

    // -------------------------------------------------------------------------
    // Builds: Canvas → Background panel → Stats text
    // -------------------------------------------------------------------------

    void BuildHUD()
    {
        // ----- Canvas --------------------------------------------------------
        GameObject canvasObj = new GameObject("CityHUD");

        Canvas canvas        = canvasObj.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder  = 100;

        // Scale the UI consistently across all screen resolutions.
        CanvasScaler scaler          = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode           = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution   = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight    = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ----- Dark background panel -----------------------------------------
        // Makes text readable against any scene background.
        GameObject panelObj   = new GameObject("Background");
        panelObj.transform.SetParent(canvasObj.transform, false);

        Image panel   = panelObj.AddComponent<Image>();
        panel.color   = new Color(0f, 0.05f, 0.1f, 0.72f); // Dark navy, 72% opaque.

        RectTransform panelRect        = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin            = new Vector2(0f, 1f); // Anchor: top-left
        panelRect.anchorMax            = new Vector2(0f, 1f);
        panelRect.pivot                = new Vector2(0f, 1f);
        panelRect.anchoredPosition     = new Vector2(10f, -10f);
        panelRect.sizeDelta            = new Vector2(280f, 175f);

        // ----- Stats text ----------------------------------------------------
        GameObject textObj   = new GameObject("StatsText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _statsText            = textObj.AddComponent<TextMeshProUGUI>();
        _statsText.fontSize   = 18f;
        _statsText.color      = Color.white;
        _statsText.richText   = true;   // Enables <b>, <color> tags used below.
        _statsText.text       = "Loading...";

        // Inset inside the panel with a small margin.
        RectTransform textRect     = textObj.GetComponent<RectTransform>();
        textRect.anchorMin         = new Vector2(0f, 1f);
        textRect.anchorMax         = new Vector2(0f, 1f);
        textRect.pivot             = new Vector2(0f, 1f);
        textRect.anchoredPosition  = new Vector2(22f, -18f);
        textRect.sizeDelta         = new Vector2(256f, 155f);
    }

    // -------------------------------------------------------------------------
    // Update — refresh stats every frame.
    // -------------------------------------------------------------------------

    void Update()
    {
        if (_statsText == null || CityManager.Instance == null) return;

        int health       = CityManager.Instance.GetCityHealth();
        int active       = CityManager.Instance.activeTrash;
        int spawned      = CityManager.Instance.totalTrashSpawned;
        int cleaned      = CityManager.Instance.totalTrashCleaned;
        int robots       = FindObjectsByType<RobotController>().Length;

        // Choose a colour for the health value:
        //   green  = healthy (70–100)
        //   yellow = warning (30–69)
        //   red    = critical (0–29)
        string healthColor = health >= 70 ? "#44FF88"
                           : health >= 30 ? "#FFD700"
                           :                "#FF4444";

        _statsText.text =
            "<b>NIGHTSHIFT CITY</b>\n"                                           +
            "─────────────────\n"                                                +
            $"Health:   <color={healthColor}><b>{health}%</b></color>\n"         +
            $"Trash:    {active}\n"                                              +
            $"Spawned:  {spawned}\n"                                             +
            $"Cleaned:  {cleaned}\n"                                             +
            $"Robots:   {robots}";
    }
}

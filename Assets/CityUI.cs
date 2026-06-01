// CityUI.cs — self-building HUD. No manual Unity Editor setup required.
// Attach to any GameObject (e.g. CityManager). Press Play — UI appears automatically.
// Requires TextMeshPro (built into Unity 6).

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityUI : MonoBehaviour
{
    private TextMeshProUGUI _text;

    void Awake() => BuildHUD();

    void BuildHUD()
    {
        // ----- Canvas --------------------------------------------------------
        GameObject canvasObj       = new GameObject("CityHUD");
        Canvas canvas              = canvasObj.AddComponent<Canvas>();
        canvas.renderMode          = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder        = 100;

        CanvasScaler scaler        = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ----- Background panel ----------------------------------------------
        GameObject panel = new GameObject("Background");
        panel.transform.SetParent(canvasObj.transform, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0.05f, 0.12f, 0.80f); // dark navy

        RectTransform panelRT          = panel.GetComponent<RectTransform>();
        panelRT.anchorMin              = new Vector2(0f, 1f);
        panelRT.anchorMax              = new Vector2(0f, 1f);
        panelRT.pivot                  = new Vector2(0f, 1f);
        panelRT.anchoredPosition       = new Vector2(10f, -10f);
        panelRT.sizeDelta              = new Vector2(290f, 230f);

        // ----- Stats text ----------------------------------------------------
        GameObject textObj = new GameObject("StatsText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _text            = textObj.AddComponent<TextMeshProUGUI>();
        _text.fontSize   = 17f;
        _text.color      = Color.white;
        _text.richText   = true;
        _text.text       = "Loading...";

        RectTransform textRT     = textObj.GetComponent<RectTransform>();
        textRT.anchorMin         = new Vector2(0f, 1f);
        textRT.anchorMax         = new Vector2(0f, 1f);
        textRT.pivot             = new Vector2(0f, 1f);
        textRT.anchoredPosition  = new Vector2(22f, -18f);
        textRT.sizeDelta         = new Vector2(266f, 210f);
    }

    void Update()
    {
        if (_text == null || CityManager.Instance == null) return;

        int health   = CityManager.Instance.GetCityHealth();
        int trash    = CityManager.Instance.activeTrash;
        int potholes = CityManager.Instance.activePotholes;
        int cleaned  = CityManager.Instance.totalTrashCleaned;
        int repaired = CityManager.Instance.totalPotholesRepaired;

        // Count each bot type separately.
        int cleaners = FindObjectsByType<RobotController>().Length;
        int repairers = FindObjectsByType<RepairBotController>().Length;

        // Health colour: green → yellow → red
        string hc = health >= 70 ? "#44FF88" : health >= 30 ? "#FFD700" : "#FF4444";

        _text.text =
            "<b>NIGHTSHIFT CITY</b>\n"
          + "──────────────────\n"
          + $"Health:    <color={hc}><b>{health}%</b></color>\n"
          + "──────────────────\n"
          + $"Trash:     {trash}  (cleaned: {cleaned})\n"
          + $"Potholes:  {potholes}  (fixed: {repaired})\n"
          + "──────────────────\n"
          + $"CleanerBots:  {cleaners}\n"
          + $"RepairBots:   {repairers}";
    }
}

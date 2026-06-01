// CityUI.cs v0.3 — self-building HUD.
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
        // ----- Canvas -------------------------------------------------------
        GameObject co         = new GameObject("CityHUD");
        Canvas canvas         = co.AddComponent<Canvas>();
        canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder   = 100;

        CanvasScaler sc       = co.AddComponent<CanvasScaler>();
        sc.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        sc.matchWidthOrHeight  = 0.5f;
        co.AddComponent<GraphicRaycaster>();

        // ----- Dark panel ---------------------------------------------------
        GameObject panel      = new GameObject("BG");
        panel.transform.SetParent(co.transform, false);
        Image bg              = panel.AddComponent<Image>();
        bg.color              = new Color(0f, 0.04f, 0.10f, 0.85f);

        RectTransform pr      = panel.GetComponent<RectTransform>();
        pr.anchorMin          = new Vector2(0f, 1f);
        pr.anchorMax          = new Vector2(0f, 1f);
        pr.pivot              = new Vector2(0f, 1f);
        pr.anchoredPosition   = new Vector2(10f, -10f);
        pr.sizeDelta          = new Vector2(310f, 270f);

        // ----- Text ---------------------------------------------------------
        GameObject to         = new GameObject("Text");
        to.transform.SetParent(co.transform, false);
        _text                 = to.AddComponent<TextMeshProUGUI>();
        _text.fontSize        = 16f;
        _text.color           = Color.white;
        _text.richText        = true;
        _text.text            = "Loading...";

        RectTransform tr      = to.GetComponent<RectTransform>();
        tr.anchorMin          = new Vector2(0f, 1f);
        tr.anchorMax          = new Vector2(0f, 1f);
        tr.pivot              = new Vector2(0f, 1f);
        tr.anchoredPosition   = new Vector2(20f, -18f);
        tr.sizeDelta          = new Vector2(290f, 250f);
    }

    void Update()
    {
        if (_text == null || CityManager.Instance == null) return;

        int health   = CityManager.Instance.GetCityHealth();
        int trash    = CityManager.Instance.activeTrash;
        int potholes = CityManager.Instance.activePotholes;
        int cleaned  = CityManager.Instance.totalTrashCleaned;
        int repaired = CityManager.Instance.totalPotholesRepaired;
        int cleaners  = FindObjectsByType<RobotController>().Length;
        int repairers = FindObjectsByType<RepairBotController>().Length;

        // Colour-code city health
        string hc = health >= 70 ? "#44FF88" : health >= 30 ? "#FFD700" : "#FF4444";

        // Status label based on thresholds
        string status, sc;
        if (health > 75)      { status = "STABLE";   sc = "#44FF88"; }
        else if (health > 40) { status = "WARNING";  sc = "#FFD700"; }
        else                  { status = "CRITICAL"; sc = "#FF4444"; }

        _text.text =
            "<b>NIGHTSHIFT CITY</b>  <size=11><color=#88AACC>v0.3</color></size>\n"
          + $"<color={sc}><b>[ {status} ]</b></color>\n"
          + "─────────────────────\n"
          + $"<b>Health:</b>    <color={hc}><b>{health}%</b></color>\n"
          + "─────────────────────\n"
          + $"<b>Trash</b>      {trash} active  /  {cleaned} cleaned\n"
          + $"<b>Potholes</b>   {potholes} active  /  {repaired} fixed\n"
          + "─────────────────────\n"
          + $"<color=#88CCFF>▶ CleanerBots</color>   {cleaners}\n"
          + $"<color=#FFAA44>▶ RepairBots</color>    {repairers}";
    }
}

using UnityEngine;

/// <summary>
/// 旅行任务区中文 OnGUI，风格贴近 MissionHUD / StealthGameHUD。
/// </summary>
public class TravelGameHUD : MonoBehaviour
{
    public Font uiFont;
    public string objective = "打卡中国地标";
    public string detail = "";
    public string bottomPrompt = "WASD 移动    走进黄色光圈";

    public bool showFeedbackBanner;
    public string feedbackTitle = "完成";
    public string feedbackSubtitle = "";

    GUIStyle title;
    GUIStyle body;
    GUIStyle banner;
    GUIStyle bannerSub;
    bool stylesReady;

    const string Glyphs =
        "点击游戏画面，开始控制镜头。目标打卡中国地标西湖断桥长城烽火台黄山迎客松。" +
        "下一站处已完成旅行任务通关三处地标可退出后续里程碑再扩展更多城市。" +
        "步行前往黄色光圈杭州八达岭观景台。WASD ：，。0123456789 / 米";

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
    }

    void Start()
    {
        if (uiFont == null)
            return;
        try
        {
            uiFont.RequestCharactersInTexture(Glyphs, 20, FontStyle.Normal);
            uiFont.RequestCharactersInTexture(Glyphs, 28, FontStyle.Normal);
            uiFont.RequestCharactersInTexture(Glyphs, 36, FontStyle.Normal);
            uiFont.RequestCharactersInTexture(Glyphs, 48, FontStyle.Normal);
            uiFont.RequestCharactersInTexture(Glyphs, 64, FontStyle.Normal);
        }
        catch (System.Exception)
        {
        }
    }

    void OnGUI()
    {
        EnsureStyles();

        if (Cursor.lockState != CursorLockMode.Locked)
            DrawCenterHint("点击游戏画面，开始控制镜头");

        DrawObjective();
        DrawPrompt();

        if (showFeedbackBanner)
            DrawBanner(feedbackTitle, feedbackSubtitle, new Color(0.05f, 0.28f, 0.12f, 0.82f));
    }

    void DrawObjective()
    {
        float w = Mathf.Min(Screen.width - 40f, 720f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, 18f, w, 78f);
        DrawPanel(bar, new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(new Rect(bar.x, bar.y + 8f, bar.width, 34f), "目标：" + objective, title);
        GUI.Label(new Rect(bar.x, bar.y + 40f, bar.width, 28f), detail, body);
    }

    void DrawPrompt()
    {
        float w = Mathf.Min(Screen.width - 24f, 980f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 72f, w, 52f);
        DrawPanel(bar, new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(bar, bottomPrompt, body);
    }

    void DrawBanner(string main, string sub, Color bg)
    {
        float w = Mathf.Min(Screen.width * 0.7f, 640f);
        float h = 180f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        DrawPanel(panel, bg);
        GUI.Label(new Rect(panel.x, panel.y + 28f, panel.width, 78f), main, banner);
        GUI.Label(new Rect(panel.x, panel.y + 108f, panel.width, 40f), sub, bannerSub);
    }

    void DrawCenterHint(string text)
    {
        float w = 520f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.62f, w, 48f);
        DrawPanel(panel, new Color(0f, 0f, 0f, 0.45f));
        GUI.Label(panel, text, body);
    }

    void DrawPanel(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    void EnsureStyles()
    {
        if (stylesReady)
            return;
        title = Make(30, TextAnchor.MiddleCenter, Color.white);
        body = Make(20, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.9f));
        banner = Make(52, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.15f));
        bannerSub = Make(22, TextAnchor.MiddleCenter, Color.white);
        stylesReady = true;
    }

    GUIStyle Make(int size, TextAnchor anchor, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        if (uiFont != null)
            style.font = uiFont;
        style.fontSize = Mathf.RoundToInt(size * Screen.height / 1080f);
        if (style.fontSize < 14)
            style.fontSize = 14;
        style.alignment = anchor;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }
}

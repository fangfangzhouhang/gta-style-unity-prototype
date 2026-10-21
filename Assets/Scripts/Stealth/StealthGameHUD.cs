using UnityEngine;

/// <summary>
/// 考场 / 学校潜行用的中文 OnGUI，风格贴近 MissionHUD。
/// </summary>
public class StealthGameHUD : MonoBehaviour
{
    public Font uiFont;
    public string objective = "潜行逃出";
    public string detail = "";
    public string bottomPrompt = "WASD 移动    Ctrl/C 蹲走    避开监考视野";

    public bool showFailBanner;
    public bool showSuccessBanner;
    public string failTitle = "被发现了";
    public string failSubtitle = "正在回到检查点…";
    public string successTitle = "逃出考场";
    public string successSubtitle = "继续前进";

    ThirdPersonController player;

    GUIStyle title;
    GUIStyle body;
    GUIStyle prompt;
    GUIStyle banner;
    GUIStyle bannerSub;
    bool stylesReady;

    const string Glyphs =
        "点击游戏画面，开始控制镜头。目标潜行逃出考场学校走廊校门。" +
        "被发现了正在回到检查点。逃出考场逃出学校继续前往学校大门。" +
        "正在前往交通枢纽请打开菜单生成交通枢纽场景学校段完成。" +
        "步行移动鼠标蹲走冲刺跳跃避开监考巡逻视野。利用课桌柜子掩体。" +
        "WASD Ctrl C Shift Esc ：，。0123456789 米";

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
        player = UnityCompat.FindOne<ThirdPersonController>();
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
            uiFont.RequestCharactersInTexture(Glyphs, 72, FontStyle.Normal);
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

        if (showFailBanner)
            DrawBanner(failTitle, failSubtitle, new Color(0.45f, 0.08f, 0.08f, 0.82f));
        else if (showSuccessBanner)
            DrawBanner(successTitle, successSubtitle, new Color(0.05f, 0.28f, 0.12f, 0.82f));
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
        string line = bottomPrompt;
        if (player != null && player.IsCrouching)
            line = "蹲走中    WASD 移动    松开 Ctrl/C 站起    利用掩体躲避视线";

        float w = Mathf.Min(Screen.width - 24f, 980f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 72f, w, 52f);
        bool crouch = player != null && player.IsCrouching;
        DrawPanel(bar, crouch ? new Color(0.12f, 0.22f, 0.35f, 0.82f) : new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(bar, line, crouch ? prompt : body);
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
        prompt = Make(26, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.25f));
        banner = Make(64, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.15f));
        bannerSub = Make(24, TextAnchor.MiddleCenter, Color.white);
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
        style.fontStyle = FontStyle.Normal;
        style.alignment = anchor;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }
}

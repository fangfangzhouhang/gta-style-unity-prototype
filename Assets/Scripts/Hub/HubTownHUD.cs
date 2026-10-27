using UnityEngine;

/// <summary>
/// 现实城镇中文 OnGUI。
/// </summary>
public class HubTownHUD : MonoBehaviour
{
    public Font uiFont;
    public string objective = "准备旅行";
    public string detail = "";
    public string bottomPrompt = "WASD 移动  空格跳跃  E 交互  M 地图  Esc 暂停";
    public bool showTipBanner;
    public string tipTitle = "提示";
    public string tipSubtitle = "";

    GUIStyle title;
    GUIStyle body;
    GUIStyle banner;
    GUIStyle bannerSub;
    bool stylesReady;

    const string Glyphs =
        "点击游戏画面开始控制镜头目标准备旅行回家留条买手机赚钱购票。" +
        "资金手机留条地图已解锁路边摊任务点交通枢纽家庭。" +
        "按交互空格跳跃暂停查看地图景点西湖长城黄山枢纽。" +
        "WASD E M Esc ：，。0123456789";

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
            uiFont.RequestCharactersInTexture(Glyphs, 48, FontStyle.Normal);
        }
        catch { }
    }

    void OnGUI()
    {
        if (UnityCompat.FindOne<CampusFlowController>() != null)
            return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;
        EnsureStyles();
        if (Cursor.lockState != CursorLockMode.Locked)
            DrawCenterHint("点击游戏画面，开始控制镜头");

        GameProgress gp = GameProgress.Ensure();
        float w = Mathf.Min(Screen.width - 40f, 760f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, 18f, w, 78f);
        DrawPanel(bar, new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(new Rect(bar.x, bar.y + 8f, bar.width, 34f), "目标：" + objective, title);
        GUI.Label(new Rect(bar.x, bar.y + 40f, bar.width, 28f),
            detail + "　资金：" + gp.money + "　手机：" + (gp.hasPhone ? "有" : "无") +
            "　留条：" + (gp.leftNote ? "已留" : "未留"), body);

        float pw = Mathf.Min(Screen.width - 24f, 980f);
        Rect prompt = new Rect((Screen.width - pw) * 0.5f, Screen.height - 72f, pw, 52f);
        DrawPanel(prompt, new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(prompt, bottomPrompt, body);

        if (showTipBanner)
        {
            float bw = Mathf.Min(Screen.width * 0.7f, 640f);
            Rect panel = new Rect((Screen.width - bw) * 0.5f, (Screen.height - 160f) * 0.5f, bw, 160f);
            DrawPanel(panel, new Color(0.28f, 0.18f, 0.05f, 0.85f));
            GUI.Label(new Rect(panel.x, panel.y + 28f, panel.width, 60f), tipTitle, banner);
            GUI.Label(new Rect(panel.x, panel.y + 96f, panel.width, 40f), tipSubtitle, bannerSub);
        }
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
        Color prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prev;
    }

    void EnsureStyles()
    {
        if (stylesReady)
            return;
        title = Make(28, TextAnchor.MiddleCenter, Color.white);
        body = Make(18, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.9f));
        banner = Make(48, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.15f));
        bannerSub = Make(22, TextAnchor.MiddleCenter, Color.white);
        stylesReady = true;
    }

    GUIStyle Make(int size, TextAnchor anchor, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        if (uiFont != null)
            style.font = uiFont;
        style.fontSize = Mathf.Max(14, Mathf.RoundToInt(size * Screen.height / 1080f));
        style.alignment = anchor;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }
}

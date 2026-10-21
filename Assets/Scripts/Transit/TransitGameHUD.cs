using UnityEngine;

/// <summary>
/// 交通枢纽中文 OnGUI：目标条、购票面板、提示横幅。风格贴近 StealthGameHUD。
/// </summary>
public class TransitGameHUD : MonoBehaviour
{
    public Font uiFont;
    public TransitFlowController flow;

    public string objective = "购票出行";
    public string detail = "";
    public string bottomPrompt = "WASD 移动    E 购票";

    public bool ticketPanelOpen;
    public bool showTipBanner;
    public bool showSuccessBanner;
    public string tipTitle = "提示";
    public string tipSubtitle = "";
    public string successTitle = "检票通过";
    public string successSubtitle = "继续前进";

    GUIStyle title;
    GUIStyle body;
    GUIStyle prompt;
    GUIStyle banner;
    GUIStyle bannerSub;
    GUIStyle button;
    bool stylesReady;

    const string Glyphs =
        "点击游戏画面，开始控制镜头。目标在综合交通枢纽购票出行检票进站。" +
        "靠近橙色高铁窗口按选车次右侧为机场值机占位。持有走近蓝色闸机。" +
        "购票成功已获得车票无法进站请先到窗口核验通过正在前往旅行任务区。" +
        "请打开菜单生成旅行场景。确认关闭取消自由出走远方。" +
        "WASD E Esc ：，。0123456789 G-D-C 米";

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
        if (flow == null)
            flow = UnityCompat.FindOne<TransitFlowController>();
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

        if (!ticketPanelOpen && Cursor.lockState != CursorLockMode.Locked)
            DrawCenterHint("点击游戏画面，开始控制镜头");

        DrawObjective();
        DrawPrompt();

        if (ticketPanelOpen)
            DrawTicketPanel();
        else if (showSuccessBanner)
            DrawBanner(successTitle, successSubtitle, new Color(0.05f, 0.28f, 0.12f, 0.82f));
        else if (showTipBanner)
            DrawBanner(tipTitle, tipSubtitle, new Color(0.28f, 0.18f, 0.05f, 0.82f));
    }

    void DrawTicketPanel()
    {
        float w = Mathf.Min(Screen.width * 0.55f, 520f);
        float h = 320f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        DrawPanel(panel, new Color(0.06f, 0.1f, 0.16f, 0.92f));

        GUI.Label(new Rect(panel.x, panel.y + 16f, panel.width, 40f), "选择车次", title);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 56f, panel.width - 40f, 28f),
            "高铁窗口 · 确认后获得车票", body);

        string[] trains = TransitFlowController.TrainChoices;
        float by = panel.y + 100f;
        for (int i = 0; i < trains.Length; i++)
        {
            Rect br = new Rect(panel.x + 40f, by + i * 48f, panel.width - 80f, 40f);
            if (GUI.Button(br, trains[i], button))
            {
                if (flow == null)
                    flow = UnityCompat.FindOne<TransitFlowController>();
                if (flow != null)
                    flow.ConfirmBuyTicket(trains[i]);
            }
        }

        Rect close = new Rect(panel.x + 40f, panel.y + h - 52f, panel.width - 80f, 36f);
        if (GUI.Button(close, "关闭（Esc）", button))
        {
            if (flow == null)
                flow = UnityCompat.FindOne<TransitFlowController>();
            if (flow != null)
                flow.CloseTicketPanel();
        }
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
        if (ticketPanelOpen)
            return;
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
        prompt = Make(26, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.25f));
        banner = Make(56, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.15f));
        bannerSub = Make(22, TextAnchor.MiddleCenter, Color.white);
        button = new GUIStyle(GUI.skin.button);
        if (uiFont != null)
            button.font = uiFont;
        button.fontSize = Mathf.RoundToInt(22 * Screen.height / 1080f);
        if (button.fontSize < 14)
            button.fontSize = 14;
        button.alignment = TextAnchor.MiddleCenter;
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

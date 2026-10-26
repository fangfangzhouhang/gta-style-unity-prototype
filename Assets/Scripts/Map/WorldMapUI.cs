using UnityEngine;

/// <summary>
/// 逃出教学楼后解锁的简易世界地图：景点与枢纽图标 + 说明。
/// </summary>
public class WorldMapUI : MonoBehaviour
{
    public Font uiFont;
    public bool IsOpen { get; private set; }

    struct MapPin
    {
        public string name;
        public string desc;
        public Vector2 uiPos; // 相对面板 0-1
        public bool hub;
    }

    MapPin[] pins;
    int selected = -1;
    GUIStyle title;
    GUIStyle body;
    GUIStyle pinStyle;
    bool stylesReady;

    const string Glyphs =
        "世界地图关闭景点交通枢纽西湖断桥长城烽火台黄山迎客松综合站。" +
        "说明点击图标查看按M开关尚未逃出教学楼无法查看。" +
        "Esc ：，。0123456789";

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
        pins = new[]
        {
            new MapPin { name = "综合交通枢纽", desc = "购票检票出发地（需手机看价、花钱买票、已留条）", uiPos = new Vector2(0.5f, 0.72f), hub = true },
            new MapPin { name = "西湖断桥", desc = "旅行打卡地标", uiPos = new Vector2(0.28f, 0.38f), hub = false },
            new MapPin { name = "长城烽火台", desc = "旅行打卡地标", uiPos = new Vector2(0.55f, 0.28f), hub = false },
            new MapPin { name = "黄山迎客松", desc = "旅行打卡地标", uiPos = new Vector2(0.72f, 0.48f), hub = false },
            new MapPin { name = "家", desc = "潜行留纸条处", uiPos = new Vector2(0.22f, 0.62f), hub = false },
        };
    }

    public void Toggle()
    {
        if (!GameProgress.Ensure().mapUnlocked)
            return;
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if(CampusJournal.Opened || (PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen))return;
        var chapter=UnityCompat.FindOne<CampusExamChapter>();if(chapter!=null && chapter.KeypadOpen)return;
        if (!GameProgress.Ensure().mapUnlocked)
            return;
        IsOpen = true;
        selected = -1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            motor.controlEnabled = false;
    }

    public void Close()
    {
        IsOpen = false;
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            motor.controlEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOpen)
            return;
        if(Input.GetKeyDown(KeyCode.Escape))CampusJournal.ConsumeEscape();
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            Close();
    }

    void OnGUI()
    {
        if (!IsOpen)
            return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;
        EnsureStyles();

        float w = Mathf.Min(Screen.width * 0.7f, 720f);
        float h = Mathf.Min(Screen.height * 0.7f, 480f);
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        DrawPanel(panel, new Color(0.08f, 0.16f, 0.12f, 0.94f));
        GUI.Label(new Rect(panel.x, panel.y + 12f, panel.width, 36f), "世界地图（已解锁）", title);

        for (int i = 0; i < pins.Length; i++)
        {
            MapPin p = pins[i];
            float px = panel.x + p.uiPos.x * panel.width - 50f;
            float py = panel.y + p.uiPos.y * panel.height - 16f;
            Rect br = new Rect(px, py, 100f, 32f);
            string label = (p.hub ? "枢纽·" : "景点·") + p.name;
            if (GUI.Button(br, label, pinStyle))
                selected = i;
        }

        string info = selected >= 0 ? pins[selected].name + "：" + pins[selected].desc : "点击图标查看说明　按 M / Esc 关闭";
        GUI.Label(new Rect(panel.x + 20f, panel.y + h - 70f, panel.width - 40f, 50f), info, body);

        if (GUI.Button(new Rect(panel.x + panel.width - 120f, panel.y + 12f, 100f, 32f), "关闭", pinStyle))
            Close();
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
        if (uiFont != null)
        {
            try
            {
                uiFont.RequestCharactersInTexture(Glyphs, 18, FontStyle.Normal);
                uiFont.RequestCharactersInTexture(Glyphs, 28, FontStyle.Normal);
            }
            catch { }
        }
        title = Make(28, TextAnchor.MiddleCenter, Color.white);
        body = Make(18, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.88f));
        pinStyle = new GUIStyle(GUI.skin.button);
        if (uiFont != null)
            pinStyle.font = uiFont;
        pinStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(14 * Screen.height / 1080f));
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

using UnityEngine;

/// <summary>
/// 用 OnGUI 画任务提示，不依赖 TextMeshPro 或额外 UI 包。
/// 中文来自 Resources/UIFont（Droid Sans Fallback）。
/// </summary>
public class MissionHUD : MonoBehaviour
{
    public Font uiFont;
    public PlayerVehicleSwitch controls;
    public MissionCheckpoint mission;
    public VehicleController vehicle;
    public ThirdPersonCamera cameraRig;

    GUIStyle title;
    GUIStyle body;
    GUIStyle prompt;
    GUIStyle banner;
    GUIStyle bannerSub;
    bool stylesReady;

    const string Glyphs =
        "点击游戏画面，开始控制镜头。目标：开车前往黄色光柱。任务完成。沿着道路直行。" +
        "距离米。你已到达检查点。驾驶中油门转向空格刹车下车。按上车。" +
        "步行移动鼠标冲刺跳跃靠近红车。WASD Shift F km/h 0123456789 ：，。";

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");

        if (controls == null)
            controls = UnityCompat.FindOne<PlayerVehicleSwitch>();
        if (mission == null)
            mission = UnityCompat.FindOne<MissionCheckpoint>();
        if (vehicle == null)
            vehicle = UnityCompat.FindOne<VehicleController>();
        if (cameraRig == null)
            cameraRig = UnityCompat.FindOne<ThirdPersonCamera>();
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
            // 字体不是动态字体时忽略，英文回退仍然能显示。
        }
    }

    void OnGUI()
    {
        EnsureStyles();

        if (Cursor.lockState != CursorLockMode.Locked)
            DrawCenterHint("点击游戏画面，开始控制镜头");

        DrawObjective();
        DrawPrompt();

        if (mission != null && mission.IsComplete)
            DrawComplete();
    }

    void DrawObjective()
    {
        float w = Mathf.Min(Screen.width - 40f, 720f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, 18f, w, 78f);
        DrawPanel(bar, new Color(0f, 0f, 0f, 0.55f));

        string objective = mission != null ? mission.objective : "开车前往黄色光柱";
        if (mission != null && mission.IsComplete)
            objective = "任务完成";

        GUI.Label(new Rect(bar.x, bar.y + 8f, bar.width, 34f), "目标：" + objective, title);

        string detail = "沿着道路直行";
        if (mission != null && !mission.IsComplete)
        {
            Vector3 from = controls != null ? controls.transform.position : Vector3.zero;
            if (controls != null && controls.IsDriving && vehicle != null)
                from = vehicle.transform.position;
            float meters = HorizontalDistance(from, mission.BeaconWorldPosition);
            detail = "距离 " + Mathf.CeilToInt(meters).ToString() + " 米";
        }
        else if (mission != null && mission.IsComplete)
        {
            detail = "你已到达检查点";
        }

        GUI.Label(new Rect(bar.x, bar.y + 40f, bar.width, 28f), detail, body);
    }

    void DrawPrompt()
    {
        string line;
        if (controls != null && controls.IsDriving)
        {
            int kmh = vehicle != null ? Mathf.RoundToInt(vehicle.PlanarSpeed * 3.6f) : 0;
            line = "驾驶中  " + kmh + " km/h    W/S 油门    A/D 转向    空格刹车    F 下车";
        }
        else if (controls != null && controls.IsCloseEnough())
        {
            line = "按 F 上车";
        }
        else
        {
            line = "步行：WASD 移动    鼠标转向    Shift 冲刺    空格跳跃    靠近红车按 F";
        }

        float w = Mathf.Min(Screen.width - 24f, 980f);
        Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 72f, w, 52f);
        bool nearCar = controls != null && !controls.IsDriving && controls.IsCloseEnough();
        DrawPanel(bar, nearCar ? new Color(0.35f, 0.22f, 0.02f, 0.82f) : new Color(0f, 0f, 0f, 0.55f));
        GUI.Label(bar, line, nearCar ? prompt : body);
    }

    void DrawComplete()
    {
        float w = Mathf.Min(Screen.width * 0.7f, 640f);
        float h = 180f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        DrawPanel(panel, new Color(0.05f, 0.05f, 0.05f, 0.78f));
        GUI.Label(new Rect(panel.x, panel.y + 28f, panel.width, 78f), "任务完成", banner);
        GUI.Label(new Rect(panel.x, panel.y + 108f, panel.width, 28f), "你已到达检查点", bannerSub);
        GUI.Label(new Rect(panel.x, panel.y + 138f, panel.width, 28f), "MISSION COMPLETE", FallbackStyle());
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

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void EnsureStyles()
    {
        if (stylesReady)
            return;

        // 这份字体没有粗体字重，统一用 Normal，避免中文变成方框。
        title = Make(30, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
        body = Make(20, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.9f));
        prompt = Make(26, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.25f));
        banner = Make(64, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.15f));
        bannerSub = Make(24, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
        stylesReady = true;
    }

    GUIStyle FallbackStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.Max(14, Mathf.RoundToInt(18f * Screen.height / 1080f));
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = new Color(1f, 0.95f, 0.8f);
        return style;
    }

    GUIStyle Make(int size, FontStyle fontStyle, TextAnchor anchor, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        if (uiFont != null)
            style.font = uiFont;
        style.fontSize = Mathf.RoundToInt(size * Screen.height / 1080f);
        if (style.fontSize < 14)
            style.fontSize = 14;
        style.fontStyle = fontStyle;
        style.alignment = anchor;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }
}

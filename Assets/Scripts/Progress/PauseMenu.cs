using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Esc 暂停菜单：继续 / 保存 / 读取 / 退出到标题 / 退出游戏。
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public Font uiFont;
    public string titleSceneName = "HubTown";
    public bool blockWhenPanelOpen = true;

    public bool IsOpen { get; private set; }

    string statusLine = "";
    float statusUntil;
    GUIStyle title;
    GUIStyle body;
    GUIStyle button;
    bool stylesReady;
    bool wasCursorLocked;

    const string Glyphs =
        "暂停继续保存读取退出到标题退出游戏存档成功读取成功没有存档。" +
        "资金手机留条地图请先保存再退出。联机大厅骨架：" +
        "Esc WASD 空格跳跃，。0123456789";

    public static PauseMenu Ensure()
    {
        if (Instance != null)
            return Instance;
        GameObject go = new GameObject("PauseMenu");
        return go.AddComponent<PauseMenu>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
        GameProgress.Ensure();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOpen)
                Close();
            else if (CanOpen())
                Open();
        }
    }

    bool CanOpen()
    {
        if(CampusJournal.Opened || CampusJournal.EscapeFrame==Time.frameCount)return false;
        var chapter=UnityCompat.FindOne<CampusExamChapter>();if(chapter!=null && chapter.KeypadOpen)return false;
        var map=UnityCompat.FindOne<WorldMapUI>();if(map!=null && map.IsOpen)return false;
        // 购票面板打开时 Esc 由 Transit 处理
        TransitFlowController transit = UnityCompat.FindOne<TransitFlowController>();
        if (transit != null && transit.TicketPanelOpen)
            return false;
        return true;
    }

    public void Open()
    {
        if(IsOpen || !CanOpen())return;
        IsOpen = true;
        wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        SetPlayerControl(false);
        statusLine = "";
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        SetPlayerControl(true);
        if (wasCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    static void SetPlayerControl(bool enabled)
    {
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            motor.controlEnabled = enabled;
    }

    void OnGUI()
    {
        if (!IsOpen)
            return;
        EnsureStyles();

        float w = Mathf.Min(Screen.width * 0.42f, 420f);
        float h = 410f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        DrawPanel(panel, new Color(0.05f, 0.07f, 0.12f, 0.94f));
        GUI.Label(new Rect(panel.x, panel.y + 16f, panel.width, 40f), "暂停", title);

        GameProgress gp = GameProgress.Ensure();
        GUI.Label(new Rect(panel.x + 20f, panel.y + 56f, panel.width - 40f, 28f),
            "资金：" + gp.money + "　手机：" + (gp.hasPhone ? "有" : "无") +
            "　留条：" + (gp.leftNote ? "已留" : "未留"), body);

        float by = panel.y + 100f;
        if (MenuButton(panel, ref by, "继续"))
            Close();
        if (MenuButton(panel, ref by, "保存"))
        {
            SaveSystem.Save();
            ShowStatus("存档成功");
        }
        if (MenuButton(panel, ref by, "读取"))
        {
            if (SaveSystem.HasSave())
            {
                Time.timeScale = 1f;
                IsOpen = false;
                SaveSystem.LoadAndApply(true);
            }
            else
                ShowStatus("没有存档");
        }
        if (MenuButton(panel, ref by, "联机大厅"))TrySwitchMode("NetLobby");
        if (MenuButton(panel, ref by, "退出到标题"))
        {
            string scene=Application.CanStreamedLevelBeLoaded(titleSceneName)?titleSceneName:"OpenWorld";
            TrySwitchMode(scene);
        }
        if (MenuButton(panel, ref by, "退出游戏"))
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (!string.IsNullOrEmpty(statusLine) && Time.unscaledTime < statusUntil)
            GUI.Label(new Rect(panel.x, panel.y + h - 36f, panel.width, 28f), statusLine, body);
    }

    public bool CanChangeMode { get {var chapter=UnityCompat.FindOne<CampusDepartureChapter>();return chapter==null || chapter.State.completed;} }
    public bool TrySwitchMode(string scene)
    {
        if(!CanChangeMode){ShowStatus("请完成章节后再切换模式。");return false;}
        if(!Application.CanStreamedLevelBeLoaded(scene)){ShowStatus("场景未在 Build Settings，请先生成。");return false;}
        Time.timeScale=1;IsOpen=false;SceneManager.LoadScene(scene);return true;
    }

    bool MenuButton(Rect panel, ref float y, string label)
    {
        Rect r = new Rect(panel.x + 40f, y, panel.width - 80f, 36f);
        y += 42f;
        return GUI.Button(r, label, button);
    }

    void ShowStatus(string msg)
    {
        statusLine = msg;
        statusUntil = Time.unscaledTime + 2f;
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
        if (uiFont != null)
        {
            try
            {
                uiFont.RequestCharactersInTexture(Glyphs, 22, FontStyle.Normal);
                uiFont.RequestCharactersInTexture(Glyphs, 32, FontStyle.Normal);
            }
            catch { }
        }
        title = Make(32, TextAnchor.MiddleCenter, Color.white);
        body = Make(18, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.88f));
        button = new GUIStyle(GUI.skin.button);
        if (uiFont != null)
            button.font = uiFont;
        button.fontSize = Mathf.Max(14, Mathf.RoundToInt(20 * Screen.height / 1080f));
        button.alignment = TextAnchor.MiddleCenter;
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

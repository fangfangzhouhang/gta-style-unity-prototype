using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Host / Client 简易中文 UI。可放在联机大厅，或 Esc 旁「联机」入口加载本场景后使用。
/// 使用 Unity Transport（本地 / 局域网），无 Relay / Vivox。
/// </summary>
public class EscapeNetworkBootstrap : MonoBehaviour
{
    public static EscapeNetworkBootstrap Instance { get; private set; }

    public string address = "127.0.0.1";
    public ushort port = 7777;
    public string examSceneName = "NetExamHall";
    public bool showMenu = true;
    public bool loadExamOnHost = true;

    NetworkPlayerSpawner spawner;
    string statusLine = "未连接（骨架：非完整联机产品）";
    Font uiFont;
    GUIStyle title;
    GUIStyle body;
    GUIStyle button;
    bool stylesReady;
    string addressField;
    string portField;

    const string Glyphs =
        "联机大厅创建主机客户端断开连接地址端口进入考场骨架局域网本机双开" +
        "等待玩家说明首次打开Package请解析：" +
        "Host Client 0123456789.:，。";

    void Awake()
    {
        Instance = this;
        addressField = address;
        portField = port.ToString();
        uiFont = Resources.Load<Font>("UIFont");
        GameProgress.Ensure();
        PauseMenu.Ensure();
        EscapeNetHud.Ensure();
        EnsureNetworkManager();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public NetworkManager EnsureNetworkManager()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            var go = new GameObject("NetworkManager");
            nm = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            nm.NetworkConfig.NetworkTransport = transport;
            DontDestroyOnLoad(go);
        }

        if (spawner == null)
        {
            spawner = GetComponent<NetworkPlayerSpawner>();
            if (spawner == null)
                spawner = gameObject.AddComponent<NetworkPlayerSpawner>();
        }
        spawner.EnsurePlayerPrefabRegistered(nm);
        nm.NetworkConfig.EnableSceneManagement = true;
        return nm;
    }

    public void StartHost()
    {
        ParseFields();
        NetworkManager nm = EnsureNetworkManager();
        ApplyTransport(nm);
        if (nm.IsListening)
        {
            statusLine = "已在监听，勿重复 StartHost";
            return;
        }
        bool ok = nm.StartHost();
        statusLine = ok ? "已作为主机（Host）启动" : "Host 启动失败";
        EscapeNetHud.Ensure().SetStatus(statusLine);
        EscapeNetHud.Ensure().SetObjective("考场逃脱", "每人不同考场，逃出后到汇合点见面");
        if (ok && loadExamOnHost)
            TryLoadExamScene();
    }

    public void StartClient()
    {
        ParseFields();
        NetworkManager nm = EnsureNetworkManager();
        ApplyTransport(nm);
        if (nm.IsListening || nm.IsConnectedClient)
        {
            statusLine = "已在会话中";
            return;
        }
        bool ok = nm.StartClient();
        statusLine = ok ? "正在连接 " + address + ":" + port : "Client 启动失败";
        EscapeNetHud.Ensure().SetStatus(statusLine);
    }

    public void Shutdown()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && (nm.IsListening || nm.IsClient))
            nm.Shutdown();
        statusLine = "已断开";
        EscapeNetHud.Ensure().SetStatus(statusLine);
    }

    void ApplyTransport(NetworkManager nm)
    {
        UnityTransport utp = nm.GetComponent<UnityTransport>();
        if (utp == null)
            utp = nm.gameObject.AddComponent<UnityTransport>();
        nm.NetworkConfig.NetworkTransport = utp;
        utp.SetConnectionData(address, port);
    }

    void ParseFields()
    {
        if (!string.IsNullOrEmpty(addressField))
            address = addressField.Trim();
        ushort p;
        if (ushort.TryParse(portField, out p))
            port = p;
    }

    void TryLoadExamScene()
    {
        if (string.IsNullOrEmpty(examSceneName))
            return;
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && nm.IsServer && nm.SceneManager != null)
        {
            nm.SceneManager.LoadScene(examSceneName, LoadSceneMode.Single);
            return;
        }
        if (Application.CanStreamedLevelBeLoaded(examSceneName))
            SceneManager.LoadScene(examSceneName);
        else
            statusLine = "考场场景未进 Build Settings：" + examSceneName;
    }

    void OnGUI()
    {
        if (!showMenu)
            return;
        // If already in exam and connected, keep a compact bar only
        bool inExam = SceneManager.GetActiveScene().name == examSceneName;
        if (inExam && OfflineFallback.IsNetworkSessionActive())
        {
            DrawCompact();
            return;
        }

        EnsureStyles();
        float w = Mathf.Min(440f, Screen.width * 0.5f);
        float h = 340f;
        Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        Color prev = GUI.color;
        GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.94f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = prev;

        GUI.Label(new Rect(panel.x, panel.y + 12f, panel.width, 36f), "联机大厅（骨架）", title);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 52f, panel.width - 40f, 40f),
            statusLine, body);

        GUI.Label(new Rect(panel.x + 24f, panel.y + 100f, 80f, 28f), "地址", body);
        addressField = GUI.TextField(new Rect(panel.x + 100f, panel.y + 100f, panel.width - 140f, 28f), addressField);
        GUI.Label(new Rect(panel.x + 24f, panel.y + 136f, 80f, 28f), "端口", body);
        portField = GUI.TextField(new Rect(panel.x + 100f, panel.y + 136f, panel.width - 140f, 28f), portField);

        float by = panel.y + 180f;
        if (GUI.Button(new Rect(panel.x + 40f, by, panel.width - 80f, 36f), "创建主机（Host）", button))
            StartHost();
        by += 42f;
        if (GUI.Button(new Rect(panel.x + 40f, by, panel.width - 80f, 36f), "加入（Client）", button))
            StartClient();
        by += 42f;
        if (GUI.Button(new Rect(panel.x + 40f, by, panel.width - 80f, 36f), "断开", button))
            Shutdown();

        GUI.Label(new Rect(panel.x + 16f, panel.y + h - 48f, panel.width - 32f, 40f),
            "本机双开或局域网测；无匹配服 / Relay / 语音。", body);
    }

    void DrawCompact()
    {
        EnsureStyles();
        if (GUI.Button(new Rect(Screen.width - 140f, 12f, 128f, 32f), "断开联机", button))
            Shutdown();
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
        body = Make(16, TextAnchor.MiddleLeft, new Color(0.9f, 0.92f, 0.88f));
        button = new GUIStyle(GUI.skin.button);
        if (uiFont != null)
            button.font = uiFont;
        button.fontSize = Mathf.Max(14, Mathf.RoundToInt(18 * Screen.height / 1080f));
        stylesReady = true;
    }

    GUIStyle Make(int size, TextAnchor anchor, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        if (uiFont != null)
            style.font = uiFont;
        style.fontSize = Mathf.Max(13, Mathf.RoundToInt(size * Screen.height / 1080f));
        style.alignment = anchor;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }
}

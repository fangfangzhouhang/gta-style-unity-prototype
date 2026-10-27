using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 联机中文 HUD：状态、目标、汇合横幅。
/// </summary>
public class EscapeNetHud : MonoBehaviour
{
    public static EscapeNetHud Instance { get; private set; }

    public Font uiFont;
    public string status = "联机骨架";
    public string objective = "等待 Host / Client";
    public string detail = "";
    public bool showBanner;
    public string bannerTitle = "";
    public string bannerSubtitle = "";
    float bannerUntil;

    readonly List<NetworkPlayer> players = new List<NetworkPlayer>();

    GUIStyle title;
    GUIStyle body;
    bool stylesReady;

    const string Glyphs =
        "联机骨架主机客户端连接断开考场汇合点队友已汇合等待逃出学校分配" +
        "局域网本机双开输入地址端口开始停止状态目标详情：" +
        "Host Client WASD Esc 0123456789.:，。";

    public static EscapeNetHud Ensure()
    {
        if (Instance != null)
            return Instance;
        GameObject go = new GameObject("EscapeNetHud");
        return go.AddComponent<EscapeNetHud>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (uiFont == null)
            uiFont = Resources.Load<Font>("UIFont");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterPlayer(NetworkPlayer p)
    {
        if (p != null && !players.Contains(p))
            players.Add(p);
    }

    public void UnregisterPlayer(NetworkPlayer p)
    {
        players.Remove(p);
    }

    public void SetStatus(string s) { status = s ?? ""; }
    public void SetDetail(string s) { detail = s ?? ""; }

    public void SetObjective(string obj, string det)
    {
        objective = obj ?? "";
        detail = det ?? "";
    }

    public void ShowBanner(string t, string sub)
    {
        showBanner = true;
        bannerTitle = t ?? "";
        bannerSubtitle = sub ?? "";
        bannerUntil = Time.unscaledTime + 4f;
    }

    void Update()
    {
        if (showBanner && Time.unscaledTime > bannerUntil)
            showBanner = false;
    }

    void OnGUI()
    {
        EnsureStyles();
        float pad = 16f;
        GUI.Label(new Rect(pad, pad, Screen.width - pad * 2f, 28f), "【联机】" + status, body);
        GUI.Label(new Rect(pad, pad + 28f, Screen.width - pad * 2f, 28f), "目标：" + objective, body);
        if (!string.IsNullOrEmpty(detail))
            GUI.Label(new Rect(pad, pad + 56f, Screen.width - pad * 2f, 48f), detail, body);

        GUI.Label(new Rect(pad, Screen.height - 36f, Screen.width - pad * 2f, 28f),
            "玩家数：" + players.Count + "　WASD 移动　Esc 暂停/联机菜单", body);

        if (showBanner)
        {
            Rect r = new Rect(Screen.width * 0.2f, Screen.height * 0.35f, Screen.width * 0.6f, 100f);
            Color prev = GUI.color;
            GUI.color = new Color(0.05f, 0.12f, 0.08f, 0.92f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(r.x, r.y + 12f, r.width, 40f), bannerTitle, title);
            GUI.Label(new Rect(r.x, r.y + 52f, r.width, 36f), bannerSubtitle, body);
        }
    }

    void EnsureStyles()
    {
        if (stylesReady)
            return;
        if (uiFont != null)
        {
            try
            {
                uiFont.RequestCharactersInTexture(Glyphs, 20, FontStyle.Normal);
                uiFont.RequestCharactersInTexture(Glyphs, 30, FontStyle.Normal);
            }
            catch { }
        }
        title = Make(30, TextAnchor.MiddleCenter, Color.white);
        body = Make(18, TextAnchor.UpperLeft, new Color(0.92f, 0.94f, 0.9f));
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

using UnityEngine;

/// <summary>连通地图唯一绘制入口；保留旧 HUD 作为各流程的消息数据。</summary>
public class CampusHUD : MonoBehaviour
{
    CampusFlowController flow;
    HubTownFlowController hub;
    WorldMapUI map;
    ZoneTeleporter[] passages;
    LeaveNoteInteract note;
    BuyPhoneInteract phone;
    CampusMotorcycle motorcycle;
    CampusEscapeDirector escape;
    ScenePortalInteract[] portals;
    GUIStyle title, body, prompt;
    Font font;

    void Start()
    {
        flow = GetComponent<CampusFlowController>();
        hub = GetComponent<HubTownFlowController>();
        map = GetComponent<WorldMapUI>();
        passages = UnityCompat.FindAll<ZoneTeleporter>();
        portals = UnityCompat.FindAll<ScenePortalInteract>();
        note = UnityCompat.FindOne<LeaveNoteInteract>();
        phone = UnityCompat.FindOne<BuyPhoneInteract>();
        motorcycle = UnityCompat.FindOne<CampusMotorcycle>();
        escape = UnityCompat.FindOne<CampusEscapeDirector>();
        font = Resources.Load<Font>("UIFont");
    }

    GUIStyle Style(int size, Color color)
    {
        return new GUIStyle(GUI.skin.label) { font = font, fontSize = size,
            alignment = TextAnchor.MiddleLeft, wordWrap = true,
            normal = { textColor = color } };
    }

    void OnGUI()
    {
        if (flow == null || flow.hud == null ||
            (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen) || (map != null && map.IsOpen)) return;
        if (title == null)
        {
            title = Style(24, Color.white);
            body = Style(17, new Color(0.82f, 0.88f, 0.86f));
            prompt = Style(20, new Color(1f, 0.87f, 0.47f));
        }
        // 固定设计尺寸后等比缩放，小窗口不挤压行高。
        Matrix4x4 previous = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
        float width = Screen.width / scale, height = Screen.height / scale;
        bool town = flow.CurrentZone == CampusFlowController.Zone.Hub;
        StealthGameHUD state = flow.hud;
        string objective = state.objective, detail = state.detail;
        if(escape!=null && !flow.HasEscapedSchool){objective=escape.Objective;detail=escape.Detail;}
        if (town && hub != null && hub.hud != null)
        {
            objective = hub.hud.objective;
            detail = hub.hud.detail;
        }
        Panel(new Rect(24, 22, 660, 112));
        GUI.Label(new Rect(42, 30, 624, 38), "目标  " + objective, title);
        GUI.Label(new Rect(42, 70, 624, 54), detail, body);
        if (town)
        {
            var gp = GameProgress.Ensure();
            Panel(new Rect(width - 330, 22, 306, 76));
            GUI.Label(new Rect(width - 312, 30, 270, 60), "旅费 ¥" + gp.money + "   手机：" + (gp.hasPhone ? "有" : "无") +
                "\n纸条：" + (gp.leftNote ? "已留" : "未留"), body);
        }
        bool banner = state.showFailBanner || state.showSuccessBanner ||
            (town && hub != null && hub.hud != null && hub.hud.showTipBanner);
        if (banner)
        {
            string main = state.showFailBanner ? state.failTitle : state.successTitle;
            string sub = state.showFailBanner ? state.failSubtitle : state.successSubtitle;
            if (!state.showFailBanner && !state.showSuccessBanner && hub != null)
            { main = hub.hud.tipTitle; sub = hub.hud.tipSubtitle; }
            Rect area = new Rect((width - 660) / 2, height * 0.38f, 660, 148);
            Panel(area);
            GUI.Label(new Rect(area.x + 24, area.y + 12, 612, 44), main, title);
            GUI.Label(new Rect(area.x + 24, area.y + 62, 612, 72), sub, body);
        }
        else
        {
            string near = NearbyPrompt();
            if (near.Length == 0 && Cursor.lockState != CursorLockMode.Locked)
                near = "点击游戏画面，开始控制镜头";
            if (near.Length > 0)
            {
                Panel(new Rect((width - 780) / 2, height - 134, 780, 58));
                GUI.Label(new Rect((width - 744) / 2, height - 130, 744, 50), near, prompt);
            }
        }
        Panel(new Rect(24, height - 62, width - 48, 42));
        GUI.Label(new Rect(42, height - 60, width - 84, 38),
            motorcycle!=null && motorcycle.IsRiding ? "W/S 油门/减速   A/D 转向   空格 刹车   F 低速下车   M 地图   Esc 暂停" :
            "WASD 移动   Shift 奔跑   空格 跳跃   Ctrl/C 蹲走   E 交互   F 骑车   M 地图   Esc 暂停", body);
        GUI.matrix = previous;
    }

    string NearbyPrompt()
    {
        if (flow.IsBusy) return "";
        if (motorcycle != null && motorcycle.IsRiding)
            return "摩托车  " + motorcycle.SpeedKmh.ToString("F0") + " km/h · 刹车减速至 9 km/h 以下，按 F 下车";
        if (motorcycle != null && motorcycle.CanMount) return "F 骑上摩托车 · W 油门 / 空格刹车";
        if(escape!=null){string clue=escape.NearbyPrompt();if(clue.Length>0)return clue;}
        if (flow.CurrentZone == CampusFlowController.Zone.Home && note != null && !note.Done && note.PlayerInRange)
            return note.promptNear;
        if (flow.CurrentZone == CampusFlowController.Zone.Hub && phone != null && phone.PlayerInRange)
            return PhoneInventory.HasPhone ? "已有手机" : phone.LastTip;
        ZoneTeleporter nearest = null;
        float distance = float.MaxValue;
        if (passages != null) foreach (var passage in passages)
        {
            if (passage == null || !passage.PlayerInRange || passage.autoTrigger) continue;
            float d = (passage.transform.position - flow.player.position).sqrMagnitude;
            if (d < distance) { nearest = passage; distance = d; }
        }
        if (nearest != null) return nearest.prompt;
        if (portals != null) foreach (var portal in portals)
            if (portal != null && portal.PlayerInRange) return portal.promptNear;
        return "";
    }

    static void Panel(Rect rect)
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.035f, 0.065f, 0.07f, 0.88f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}

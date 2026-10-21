using UnityEngine;

/// <summary>
/// 逃出学校后的现实城镇：买手机、赚钱、回家留条、进枢纽、开地图。
/// </summary>
public class HubTownFlowController : MonoBehaviour
{
    public HubTownHUD hud;
    public WorldMapUI mapUi;
    public Transform player;

    float tipUntil;

    void Start()
    {
        GameProgress gp = GameProgress.Ensure();
        PauseMenu.Ensure();
        if (!gp.escapedSchool)
        {
            // 允许直接 Play 验收；标记为已逃出以便测城镇内容
            gp.MarkEscapedSchool();
        }
        gp.stage = GameProgress.StageHub;
        gp.mapUnlocked = true;

        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }
        if (hud == null)
            hud = UnityCompat.FindOne<HubTownHUD>();
        if (mapUi == null)
            mapUi = UnityCompat.FindOne<WorldMapUI>();
        RefreshHud();
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;

        if (tipUntil > 0f && Time.time >= tipUntil)
        {
            tipUntil = 0f;
            if (hud != null)
                hud.showTipBanner = false;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapUi == null)
                mapUi = UnityCompat.FindOne<WorldMapUI>();
            if (mapUi != null)
                mapUi.Toggle();
        }

        UpdateNearPrompt();
    }

    public void ShowTip(string title, string sub)
    {
        tipUntil = Time.time + 2.2f;
        if (hud != null)
        {
            hud.showTipBanner = true;
            hud.tipTitle = title;
            hud.tipSubtitle = sub;
        }
    }

    public void NotifyEarn(string title, int reward)
    {
        ShowTip("完成：" + title, "获得 " + reward + " 旅游资金　当前 " + Wallet.Balance);
        RefreshHud();
    }

    void UpdateNearPrompt()
    {
        if (hud == null)
            return;
        BuyPhoneInteract phone = UnityCompat.FindOne<BuyPhoneInteract>();
        if (phone != null && phone.PlayerInRange)
        {
            hud.bottomPrompt = PhoneInventory.HasPhone ? "已有手机" : phone.LastTip;
            return;
        }
        ScenePortalInteract[] portals = UnityCompat.FindAll<ScenePortalInteract>();
        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null && portals[i].PlayerInRange)
            {
                hud.bottomPrompt = portals[i].promptNear;
                return;
            }
        }
        hud.bottomPrompt = "WASD 移动  空格跳跃  E 交互  M 地图  Esc 暂停";
    }

    public void RefreshHud()
    {
        if (hud == null)
            return;
        GameProgress gp = GameProgress.Ensure();
        if (!gp.leftNote)
        {
            hud.objective = "回家留下纸条";
            hud.detail = "先去绿色「家」门潜行留条；再买手机、做黄圈任务赚钱";
        }
        else if (!gp.hasPhone)
        {
            hud.objective = "购买手机";
            hud.detail = "到橙色路边摊按 E 买手机，才能在枢纽查看票价";
        }
        else if (Wallet.Balance < 30)
        {
            hud.objective = "做任务赚旅游资金";
            hud.detail = "走进黄色任务圈赚钱，再去蓝色枢纽购票";
        }
        else
        {
            hud.objective = "前往交通枢纽购票";
            hud.detail = "蓝色枢纽门 → 有手机可看票价 → 花钱购票出发";
        }
    }
}

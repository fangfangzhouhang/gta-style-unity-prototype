using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 交通枢纽：需留条 + 手机看价 + 资金购票 → 检票 → 旅行。
/// 逃出教学楼前不应直接进入本场景作为主线终点。
/// </summary>
public class TransitFlowController : MonoBehaviour
{
    public TransitGameHUD hud;
    public Transform player;
    public string travelSceneName = "TravelMissions";
    public float successHoldSeconds = 1.8f;
    public bool autoLoadTravel = true;

    public static readonly string[] TrainChoices =
    {
        "G-自由01",
        "D-出走02",
        "C-远方03"
    };

    public static readonly int[] TrainPrices =
    {
        80,
        50,
        30
    };

    public bool HasClearedHub { get; private set; }
    public bool TicketPanelOpen { get; private set; }
    public bool IsBusy { get { return HasClearedHub || clearing; } }

    bool clearing;
    float bannerUntil;
    float tipUntil;
    string tipText = "";

    void Start()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        GameProgress.Ensure().stage = GameProgress.StageTransit;

        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }

        if (hud == null)
            hud = UnityCompat.FindOne<TransitGameHUD>();

        RefreshHudIdle();
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;

        if (TicketPanelOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseTicketPanel();
            return;
        }

        if (!HasClearedHub && !clearing)
            UpdateNearPrompt();

        if (tipUntil > 0f && Time.time >= tipUntil)
        {
            tipUntil = 0f;
            tipText = "";
            if (hud != null && !HasClearedHub)
                hud.showTipBanner = false;
            RefreshHudIdle();
        }

        if (clearing && Time.time >= bannerUntil)
        {
            clearing = false;
            if (hud != null)
                hud.showSuccessBanner = false;
            if (autoLoadTravel)
                TryLoadTravel();
        }
    }

    public void OpenTicketPanel(TicketWindowInteract source)
    {
        if (HasClearedHub || TicketPanelOpen)
            return;

        GameProgress gp = GameProgress.Ensure();
        if (!gp.leftNote)
        {
            NotifyBlocked("还没留纸条", "先回家桌上留条（自己赚钱出去旅游），再来购票");
            return;
        }

        TicketPanelOpen = true;
        if (hud != null)
        {
            hud.ticketPanelOpen = true;
            hud.showTipBanner = false;
            hud.showSuccessBanner = false;
            hud.needPhoneForPrices = !PhoneInventory.HasPhone;
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseTicketPanel()
    {
        TicketPanelOpen = false;
        if (hud != null)
            hud.ticketPanelOpen = false;

        if (player != null && !HasClearedHub)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = true;
        }

        RefreshHudIdle();
    }

    public void ConfirmBuyTicket(string trainName)
    {
        GameProgress gp = GameProgress.Ensure();
        if (!gp.leftNote)
        {
            NotifyBlocked("还没留纸条", "先回家留条再购票");
            CloseTicketPanel();
            return;
        }
        if (!PhoneInventory.HasPhone)
        {
            NotifyBlocked("需要手机", "需要手机才能查看票价并购票");
            return;
        }

        int price = PriceOf(trainName);
        if (!Wallet.TrySpend(price))
        {
            NotifyBlocked("资金不足", "票价 " + price + "，当前资金 " + Wallet.Balance + "。回城镇做任务赚钱");
            return;
        }

        TicketInventory.Grant(trainName);
        CloseTicketPanel();
        tipText = "已购：" + trainName + "（-" + price + "）　余额 " + Wallet.Balance;
        tipUntil = Time.time + 2.2f;
        if (hud != null)
        {
            hud.showTipBanner = true;
            hud.tipTitle = "购票成功";
            hud.tipSubtitle = tipText;
        }
        SaveSystem.Save();
        RefreshHudIdle();
    }

    public static int PriceOf(string trainName)
    {
        for (int i = 0; i < TrainChoices.Length; i++)
        {
            if (TrainChoices[i] == trainName)
                return TrainPrices[i];
        }
        return 50;
    }

    public void NotifyBlocked(string title, string hint)
    {
        tipText = hint;
        tipUntil = Time.time + 2.0f;
        if (hud != null)
        {
            hud.showTipBanner = true;
            hud.tipTitle = title;
            hud.tipSubtitle = tipText;
        }
    }

    public void NotifyNoTicket(string hint)
    {
        if (HasClearedHub || TicketPanelOpen)
            return;
        NotifyBlocked("无法进站", string.IsNullOrEmpty(hint) ? "请先购票" : hint);
    }

    public void NotifyGatePassed()
    {
        if (HasClearedHub || clearing)
            return;
        if (!TicketInventory.HasTicket)
        {
            NotifyNoTicket("请先到窗口购票");
            return;
        }

        HasClearedHub = true;
        clearing = true;
        bannerUntil = Time.time + successHoldSeconds;
        TicketInventory.Consume();
        GameProgress.Ensure().stage = GameProgress.StageTravel;
        SaveSystem.Save();

        if (hud != null)
        {
            hud.ticketPanelOpen = false;
            hud.showTipBanner = false;
            hud.showSuccessBanner = true;
            hud.successTitle = "检票通过";
            hud.successSubtitle = autoLoadTravel ? "正在前往旅行任务区…" : "枢纽通关";
            hud.objective = "进入旅行";
            hud.detail = "车票已核验";
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
    }

    void TryLoadTravel()
    {
        if (string.IsNullOrEmpty(travelSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(travelSceneName))
        {
            Debug.LogWarning("旅行场景未加入 Build Settings：" + travelSceneName);
            if (hud != null)
            {
                hud.showSuccessBanner = true;
                hud.successTitle = "检票通过";
                hud.successSubtitle = "请打开菜单生成旅行任务场景";
            }
            return;
        }

        SceneManager.LoadScene(travelSceneName);
    }

    void UpdateNearPrompt()
    {
        if (hud == null || tipUntil > Time.time)
            return;

        TicketWindowInteract[] windows = UnityCompat.FindAll<TicketWindowInteract>();
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null && windows[i].PlayerInRange)
            {
                hud.bottomPrompt = windows[i].promptNear;
                return;
            }
        }

        if (TicketInventory.HasTicket)
            hud.bottomPrompt = "WASD 移动  空格跳跃  走近闸机自动检票  Esc 暂停";
        else
            hud.bottomPrompt = "WASD 移动  空格跳跃  E 购票  Esc 暂停";
    }

    public void RefreshHudIdle()
    {
        if (hud == null)
            return;

        GameProgress gp = GameProgress.Ensure();
        string status = "资金：" + gp.money + "　手机：" + (gp.hasPhone ? "有" : "无") +
                        "　留条：" + (gp.leftNote ? "已留" : "未留");

        if (TicketInventory.HasTicket)
        {
            hud.objective = "检票进站";
            hud.detail = "持有：" + TicketInventory.TicketName + "　" + status;
            hud.bottomPrompt = "WASD 移动  走近闸机自动检票";
        }
        else if (!gp.leftNote)
        {
            hud.objective = "无法购票：尚未留条";
            hud.detail = "请先回 HubTown → 家门潜行留条。" + status;
            hud.bottomPrompt = "Esc 暂停保存　回城镇完成留条";
        }
        else if (!gp.hasPhone)
        {
            hud.objective = "购票窗口需要手机";
            hud.detail = "无手机只能看到提示，看不到票价。回城镇买手机。" + status;
            hud.bottomPrompt = "E 打开窗口（仅提示）  Esc 暂停";
        }
        else
        {
            hud.objective = "查看票价并购票";
            hud.detail = "靠近橙色窗口按 E；票价花费旅游资金。" + status;
            hud.bottomPrompt = "WASD 移动  E 购票  空格跳跃  Esc 暂停";
        }
    }
}

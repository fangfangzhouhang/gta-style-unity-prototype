using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 交通枢纽流程：购票面板 → 检票 → 淡出加载旅行任务场景。
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

    public bool HasClearedHub { get; private set; }
    public bool TicketPanelOpen { get; private set; }
    public bool IsBusy { get { return HasClearedHub || clearing; } }

    bool clearing;
    float bannerUntil;
    float tipUntil;
    string tipText = "";

    void Start()
    {
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

        TicketPanelOpen = true;
        if (hud != null)
        {
            hud.ticketPanelOpen = true;
            hud.showTipBanner = false;
            hud.showSuccessBanner = false;
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
        TicketInventory.Grant(trainName);
        CloseTicketPanel();
        tipText = "已获得车票：" + trainName;
        tipUntil = Time.time + 2.2f;
        if (hud != null)
        {
            hud.showTipBanner = true;
            hud.tipTitle = "购票成功";
            hud.tipSubtitle = tipText;
        }
        RefreshHudIdle();
    }

    public void NotifyNoTicket(string hint)
    {
        if (HasClearedHub || TicketPanelOpen)
            return;

        tipText = string.IsNullOrEmpty(hint) ? "请先购票" : hint;
        tipUntil = Time.time + 1.6f;
        if (hud != null)
        {
            hud.showTipBanner = true;
            hud.tipTitle = "无法进站";
            hud.tipSubtitle = tipText;
        }
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
            Debug.LogWarning("旅行场景未加入 Build Settings：" + travelSceneName +
                             "。请用菜单 EscapeExam → 生成旅行任务场景 后再试。");
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
            hud.bottomPrompt = "WASD 移动    走近闸机自动检票";
        else
            hud.bottomPrompt = "WASD 移动    E 购票    无票无法过闸机";
    }

    public void RefreshHudIdle()
    {
        if (hud == null)
            return;

        if (TicketInventory.HasTicket)
        {
            hud.objective = "检票进站";
            hud.detail = "持有：" + TicketInventory.TicketName + "　走近蓝色闸机进站";
            hud.bottomPrompt = "WASD 移动    走近闸机自动检票";
        }
        else
        {
            hud.objective = "在综合交通枢纽购票出行";
            hud.detail = "靠近橙色高铁窗口按 E 选车次；右侧为机场值机占位";
            hud.bottomPrompt = "WASD 移动    E 购票    无票无法过闸机";
        }
    }
}

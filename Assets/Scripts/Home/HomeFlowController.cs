using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 家庭潜行：家长视野发现 → 回屋内检查点；成功留条 → 可返回现实城镇。
/// </summary>
public class HomeFlowController : MonoBehaviour
{
    public Transform checkpoint;
    public Transform player;
    public StealthGameHUD hud;
    public float failHoldSeconds = 1.6f;
    public float successHoldSeconds = 2.2f;
    public string hubSceneName = "HubTown";
    public bool autoReturnHub = true;

    public bool IsBusy { get { return failing || successPending; } }
    public bool NoteDone { get; private set; }

    bool failing;
    bool successPending;
    float bannerUntil;
    VisionConeDetector[] cones;

    void Start()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }
        if (hud == null)
            hud = UnityCompat.FindOne<StealthGameHUD>();
        BindDetectors();
        if (UnityCompat.FindOne<CampusFlowController>() != null)
        {
            NoteDone = GameProgress.Ensure().leftNote;
            return;
        }
        if (GameProgress.Ensure().leftNote)
        {
            NoteDone = true;
            RefreshHudAfterNote();
        }
        else
            RefreshHudIdle();
    }

    public void BindDetectors()
    {
        cones = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] == null)
                continue;
            cones[i].OnPlayerSpotted -= HandleSpotted;
            cones[i].OnPlayerSpotted += HandleSpotted;
        }
    }

    void OnDestroy()
    {
        if (cones == null)
            return;
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] != null)
                cones[i].OnPlayerSpotted -= HandleSpotted;
        }
    }

    void Update()
    {
        if (failing && Time.time >= bannerUntil)
            FinishFail();
        if (successPending && Time.time >= bannerUntil)
        {
            successPending = false;
            if (hud != null)
                hud.showSuccessBanner = false;
            if (player != null)
            {
                ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
                if (motor != null)
                    motor.controlEnabled = true;
            }
            PauseAgents(false);
            if (autoReturnHub)
                TryLoadHub();
            else
            {
                CampusFlowController campus = UnityCompat.FindOne<CampusFlowController>();
                if (campus != null)
                    campus.ShowTip("纸条已留下", "从屋内出口返回城镇，准备旅费和手机");
                else
                    RefreshHudAfterNote();
            }
        }
    }

    void HandleSpotted(VisionConeDetector source, Transform spotted)
    {
        // CampusWorld 由 CampusFlowController 统一处理发现，避免双重回位
        if (UnityCompat.FindOne<CampusFlowController>() != null)
            return;
        if (NoteDone || failing || successPending || SafeDiscoveryReset.IsInvulnerable)
            return;
        BeginFail();
    }

    void BeginFail()
    {
        failing = true;
        bannerUntil = Time.time + failHoldSeconds;
        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.showSuccessBanner = false;
            hud.failTitle = "被家长发现了";
            hud.failSubtitle = "正在回到屋内检查点…";
        }
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
        PauseAgents(true);
    }

    void FinishFail()
    {
        failing = false;
        if (hud != null)
            hud.showFailBanner = false;

        SafeDiscoveryReset.TryRespawn(player, checkpoint);
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = true;
        }
        PauseAgents(false);
        RefreshHudIdle();
    }

    static void PauseAgents(bool paused)
    {
        TeacherAgent[] agents = UnityCompat.FindAll<TeacherAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] != null)
                agents[i].SetPaused(paused);
        }
        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(paused);
        }
    }

    public void NotifyNoteLeft(string noteText)
    {
        if (NoteDone && successPending)
            return;
        NoteDone = true;
        successPending = true;
        bannerUntil = Time.time + successHoldSeconds;
        GameProgress.Ensure().stage = GameProgress.StageHome;

        if (hud != null)
        {
            hud.showFailBanner = false;
            hud.showSuccessBanner = true;
            hud.successTitle = "纸条已留下";
            hud.successSubtitle = "「" + noteText + "」";
            hud.objective = "回家留条完成";
            hud.detail = "可以返回城镇买手机、赚钱、购票出发";
        }
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
        PauseAgents(true);
    }

    void TryLoadHub()
    {
        if (string.IsNullOrEmpty(hubSceneName))
            return;
        if (!Application.CanStreamedLevelBeLoaded(hubSceneName))
        {
            Debug.LogWarning("HubTown 未加入 Build Settings");
            if (hud != null)
            {
                hud.showSuccessBanner = true;
                hud.successTitle = "纸条已留下";
                hud.successSubtitle = "请用菜单生成 HubTown 场景";
            }
            return;
        }
        SceneManager.LoadScene(hubSceneName);
    }

    void RefreshHudIdle()
    {
        if (hud == null)
            return;
        hud.objective = "悄悄留下纸条";
        hud.detail = "避开家长视野，走到桌上按 E 留条";
        hud.bottomPrompt = "WASD 移动  空格跳跃  Ctrl/C 蹲走  Esc 暂停";
        hud.showFailBanner = false;
        hud.showSuccessBanner = false;
    }

    void RefreshHudAfterNote()
    {
        if (hud == null)
            return;
        hud.objective = "留条已完成";
        hud.detail = "可返回城镇继续旅行准备";
        hud.bottomPrompt = "Esc 暂停　可加载 HubTown";
    }
}

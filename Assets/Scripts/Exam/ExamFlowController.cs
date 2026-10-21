using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 考场潜行流程：监听视野发现 → 失败回检查点；出口成功 → 提示并可进学校场景。
/// </summary>
public class ExamFlowController : MonoBehaviour
{
    public Transform checkpoint;
    public Transform player;
    public StealthGameHUD hud;
    public string schoolSceneName = "SchoolEscape";
    public float failHoldSeconds = 1.6f;
    public float successHoldSeconds = 1.8f;
    public bool autoLoadSchool = true;

    public bool HasEscaped { get; private set; }

    bool failing;
    bool successPending;
    float bannerUntil;
    VisionConeDetector[] cones;

    void Start()
    {
        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }

        if (hud == null)
            hud = UnityCompat.FindOne<StealthGameHUD>();

        BindDetectors();
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
            if (autoLoadSchool)
                TryLoadSchool();
        }
    }

    void HandleSpotted(VisionConeDetector source, Transform spotted)
    {
        if (HasEscaped || failing || successPending)
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
            hud.failTitle = "被发现了";
            hud.failSubtitle = "正在回到检查点…";
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }

        PausePatrols(true);
    }

    void FinishFail()
    {
        failing = false;
        if (hud != null)
            hud.showFailBanner = false;

        RespawnAtCheckpoint();
        ResetDetectors();
        PausePatrols(false);
        RefreshHudIdle();

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
            {
                motor.ForceStand();
                motor.controlEnabled = true;
                motor.ResetVerticalSpeed();
            }
        }
    }

    void RespawnAtCheckpoint()
    {
        if (player == null || checkpoint == null)
            return;

        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = false;

        player.position = checkpoint.position;
        player.rotation = checkpoint.rotation;

        if (body != null)
            body.enabled = true;

        ThirdPersonCamera cam = UnityCompat.FindOne<ThirdPersonCamera>();
        if (cam != null)
            cam.Snap();
    }

    void ResetDetectors()
    {
        VisionConeDetector[] all = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
                all[i].ResetSpot();
        }
    }

    static void PausePatrols(bool paused)
    {
        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(paused);
        }
    }

    public void NotifyExamEscaped()
    {
        if (HasEscaped || failing)
            return;

        HasEscaped = true;
        successPending = true;
        bannerUntil = Time.time + successHoldSeconds;

        if (hud != null)
        {
            hud.showFailBanner = false;
            hud.showSuccessBanner = true;
            hud.successTitle = "逃出考场";
            hud.successSubtitle = autoLoadSchool ? "正在前往学校…" : "考场段完成";
            hud.objective = "逃出考场";
            hud.detail = "成功离开考场";
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
    }

    void TryLoadSchool()
    {
        if (string.IsNullOrEmpty(schoolSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(schoolSceneName))
        {
            Debug.LogWarning("学校场景未加入 Build Settings：" + schoolSceneName + "。请用菜单 EscapeExam → 生成学校场景 后再试。");
            if (hud != null)
            {
                hud.showSuccessBanner = true;
                hud.successTitle = "逃出考场";
                hud.successSubtitle = "请打开菜单生成学校场景";
            }
            return;
        }

        SceneManager.LoadScene(schoolSceneName);
    }

    void RefreshHudIdle()
    {
        if (hud == null)
            return;
        hud.objective = "潜行逃出考场";
        hud.detail = "利用课桌掩体，避开监考视野，走到绿色出口";
        hud.bottomPrompt = "WASD 移动    Ctrl/C 蹲走    避开监考红色视野锥";
        hud.showFailBanner = false;
        hud.showSuccessBanner = false;
    }
}

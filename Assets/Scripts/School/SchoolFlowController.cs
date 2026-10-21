using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 学校逃离流程：发现 → 回检查点；校门 → 「逃出学校」→ 可选进入交通枢纽。
/// </summary>
public class SchoolFlowController : MonoBehaviour
{
    public Transform checkpoint;
    public Transform player;
    public StealthGameHUD hud;
    public float failHoldSeconds = 1.6f;
    public float successHoldSeconds = 1.8f;
    public string transitSceneName = "TransitHub";
    public bool autoLoadTransit = true;

    public bool HasEscapedSchool { get; private set; }

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
            if (autoLoadTransit)
                TryLoadTransit();
        }
    }

    void HandleSpotted(VisionConeDetector source, Transform spotted)
    {
        if (HasEscapedSchool || failing || successPending)
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

        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(true);
        }
    }

    void FinishFail()
    {
        failing = false;
        if (hud != null)
            hud.showFailBanner = false;

        if (player != null && checkpoint != null)
        {
            CharacterController body = player.GetComponent<CharacterController>();
            if (body != null)
                body.enabled = false;
            player.position = checkpoint.position;
            player.rotation = checkpoint.rotation;
            if (body != null)
                body.enabled = true;

            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
            {
                motor.ForceStand();
                motor.ResetVerticalSpeed();
                motor.controlEnabled = true;
            }
        }

        VisionConeDetector[] all = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
                all[i].ResetSpot();
        }

        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(false);
        }

        ThirdPersonCamera cam = UnityCompat.FindOne<ThirdPersonCamera>();
        if (cam != null)
            cam.Snap();

        RefreshHudIdle();
    }

    public void NotifySchoolEscaped()
    {
        if (HasEscapedSchool || failing)
            return;

        HasEscapedSchool = true;
        successPending = true;
        bannerUntil = Time.time + successHoldSeconds;

        if (hud != null)
        {
            hud.showFailBanner = false;
            hud.showSuccessBanner = true;
            hud.successTitle = "逃出学校";
            hud.successSubtitle = autoLoadTransit ? "正在前往交通枢纽…" : "学校段完成";
            hud.objective = "逃出学校";
            hud.detail = "你已离开校园";
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }

        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(true);
        }
    }

    void TryLoadTransit()
    {
        if (string.IsNullOrEmpty(transitSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(transitSceneName))
        {
            Debug.LogWarning("交通枢纽场景未加入 Build Settings：" + transitSceneName +
                             "。请用菜单 EscapeExam → 生成交通枢纽场景 后再试。");
            if (hud != null)
            {
                hud.showSuccessBanner = true;
                hud.successTitle = "逃出学校";
                hud.successSubtitle = "请打开菜单生成交通枢纽场景";
            }
            return;
        }

        SceneManager.LoadScene(transitSceneName);
    }

    void RefreshHudIdle()
    {
        if (hud == null)
            return;
        hud.objective = "逃出学校";
        hud.detail = "沿着走廊避开巡逻，走到蓝色校门";
        hud.bottomPrompt = "WASD 移动    Ctrl/C 蹲走    利用柜子掩体";
        hud.showFailBanner = false;
        hud.showSuccessBanner = false;
    }
}

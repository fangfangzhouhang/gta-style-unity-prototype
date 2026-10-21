using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 学校逃离：老师追赶抓住 → 送回检查点短暂锁定；校门成功 → 现实城镇（非 TransitHub）。
/// </summary>
public class SchoolFlowController : MonoBehaviour
{
    public Transform checkpoint;
    public Transform player;
    public StealthGameHUD hud;
    public float failHoldSeconds = 1.4f;
    public float studyHoldSeconds = 2.2f;
    public float successHoldSeconds = 1.8f;
    public string hubSceneName = "HubTown";
    public bool autoLoadHub = true;

    public bool HasEscapedSchool { get; private set; }
    public bool IsBusy { get { return failing || studying || successPending; } }

    bool failing;
    bool studying;
    bool successPending;
    float bannerUntil;
    TeacherAgent[] agents;

    void Start()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        GameProgress.Ensure().stage = GameProgress.StageSchool;

        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }

        if (hud == null)
            hud = UnityCompat.FindOne<StealthGameHUD>();

        BindTeachers();
        RefreshHudIdle();
    }

    public void BindTeachers()
    {
        agents = UnityCompat.FindAll<TeacherAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] == null)
                continue;
            agents[i].OnCaughtPlayer -= HandleCaught;
            agents[i].OnCaughtPlayer += HandleCaught;
        }
    }

    // 兼容旧 Builder 调用名
    public void BindDetectors()
    {
        BindTeachers();
    }

    void OnDestroy()
    {
        if (agents == null)
            return;
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] != null)
                agents[i].OnCaughtPlayer -= HandleCaught;
        }
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;

        if (failing && Time.time >= bannerUntil)
            BeginForcedStudy();

        if (studying && Time.time >= bannerUntil)
            FinishStudy();

        if (successPending && Time.time >= bannerUntil)
        {
            successPending = false;
            if (hud != null)
                hud.showSuccessBanner = false;
            if (autoLoadHub)
        {
            if (OfflineFallback.ShouldUseMeetupInsteadOfHub())
            {
                // 联机会话：不进 HubTown，由 MeetupZone / EscapeNetProgress 处理汇合
                if (hud != null)
                {
                    hud.successSubtitle = "联机模式：请前往汇合点与队友见面";
                }
            }
            else
                TryLoadHub();
        }
        }
    }

    void HandleCaught(TeacherAgent source, Transform spotted)
    {
        if (HasEscapedSchool || IsBusy)
            return;
        BeginCatchSequence();
    }

    void BeginCatchSequence()
    {
        failing = true;
        bannerUntil = Time.time + failHoldSeconds;
        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.showSuccessBanner = false;
            hud.failTitle = "被老师抓住了";
            hud.failSubtitle = "正在带回…";
        }
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
        PauseTeachers(true);
    }

    void BeginForcedStudy()
    {
        failing = false;
        studying = true;
        bannerUntil = Time.time + studyHoldSeconds;

        if (player != null && checkpoint != null)
        {
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
            GameProgress.Ensure().SetCheckpoint(checkpoint.position);
        }

        TeacherAgent[] all = UnityCompat.FindAll<TeacherAgent>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
                all[i].ResetToPatrol();
        }
        VisionConeDetector[] cones = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] != null)
                cones[i].ResetSpot();
        }

        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.failTitle = "被迫学习";
            hud.failSubtitle = "走廊反省片刻，再继续逃向校门…";
        }
    }

    void FinishStudy()
    {
        studying = false;
        if (hud != null)
            hud.showFailBanner = false;
        PauseTeachers(false);
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
            {
                motor.ForceStand();
                motor.ResetVerticalSpeed();
                motor.controlEnabled = true;
            }
        }
        RefreshHudIdle();
    }

    static void PauseTeachers(bool paused)
    {
        TeacherAgent[] all = UnityCompat.FindAll<TeacherAgent>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
                all[i].SetPaused(paused);
        }
        SimplePatrol[] patrols = UnityCompat.FindAll<SimplePatrol>();
        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null)
                patrols[i].SetPaused(paused);
        }
    }

    public void NotifySchoolEscaped()
    {
        if (HasEscapedSchool || IsBusy)
            return;

        HasEscapedSchool = true;
        successPending = true;
        bannerUntil = Time.time + successHoldSeconds;

        GameProgress gp = GameProgress.Ensure();
        gp.MarkEscapedSchool();
        SaveSystem.Save();

        if (hud != null)
        {
            hud.showFailBanner = false;
            hud.showSuccessBanner = true;
            hud.successTitle = "逃出学校";
            hud.successSubtitle = autoLoadHub ? "正在进入现实世界…" : "学校段完成 · 地图已解锁";
            hud.objective = "逃出学校";
            hud.detail = "现实世界旅行已解锁（先回家留条再购票）";
        }

        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
        PauseTeachers(true);
    }

    void TryLoadHub()
    {
        if (string.IsNullOrEmpty(hubSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(hubSceneName))
        {
            Debug.LogWarning("现实城镇场景未加入 Build Settings：" + hubSceneName +
                             "。请用菜单 EscapeExam → 生成现实城镇场景。");
            if (hud != null)
            {
                hud.showSuccessBanner = true;
                hud.successTitle = "逃出学校";
                hud.successSubtitle = "请打开菜单生成 HubTown（不要直接进 TransitHub）";
            }
            return;
        }

        SceneManager.LoadScene(hubSceneName);
    }

    void RefreshHudIdle()
    {
        if (hud == null)
            return;
        hud.objective = "逃出学校";
        hud.detail = "走廊老师随机巡视追赶；走到蓝色校门进入现实世界";
        hud.bottomPrompt = "WASD 移动  空格跳跃  Ctrl/C 蹲走  Esc 暂停";
        hud.showFailBanner = false;
        hud.showSuccessBanner = false;
    }
}

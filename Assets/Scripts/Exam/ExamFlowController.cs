using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 考场潜行：老师抓住 → 送回座位被迫学习 → 再逃；出口成功 → 学校。
/// </summary>
public class ExamFlowController : MonoBehaviour
{
    public Transform checkpoint;
    public Transform player;
    public StealthGameHUD hud;
    public string schoolSceneName = "SchoolEscape";
    public float failHoldSeconds = 1.4f;
    public float studyHoldSeconds = 2.6f;
    public float successHoldSeconds = 1.8f;
    public bool autoLoadSchool = true;

    public bool HasEscaped { get; private set; }
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
        GameProgress.Ensure().stage = GameProgress.StageExam;

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
            if (autoLoadSchool)
                TryLoadSchool();
        }
    }

    void HandleCaught(TeacherAgent source, Transform spotted)
    {
        if (HasEscaped || IsBusy)
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
            hud.failSubtitle = "正在送回座位…";
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

        RespawnAtCheckpoint();
        ResetTeachers();

        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.failTitle = "被迫学习";
            hud.failSubtitle = "先老实坐一会儿，再想办法逃跑…";
            hud.objective = "被迫学习中";
            hud.detail = "锁定结束后方可再次潜行逃跑";
        }
    }

    void FinishStudy()
    {
        studying = false;
        if (hud != null)
            hud.showFailBanner = false;

        PauseTeachers(false);
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

        GameProgress.Ensure().SetCheckpoint(checkpoint.position);
    }

    void ResetTeachers()
    {
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

    public void NotifyExamEscaped()
    {
        if (HasEscaped || IsBusy)
            return;

        HasEscaped = true;
        successPending = true;
        bannerUntil = Time.time + successHoldSeconds;
        GameProgress.Ensure().stage = GameProgress.StageSchool;

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
        PauseTeachers(true);
        SaveSystem.Save();
    }

    void TryLoadSchool()
    {
        if (string.IsNullOrEmpty(schoolSceneName))
            return;

        if (!Application.CanStreamedLevelBeLoaded(schoolSceneName))
        {
            Debug.LogWarning("学校场景未加入 Build Settings：" + schoolSceneName);
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
        hud.detail = "老师会随机巡视并追赶；被抓会送回座位被迫学习";
        hud.bottomPrompt = "WASD 移动  空格跳跃  Ctrl/C 蹲走  Esc 暂停";
        hud.showFailBanner = false;
        hud.showSuccessBanner = false;
    }
}

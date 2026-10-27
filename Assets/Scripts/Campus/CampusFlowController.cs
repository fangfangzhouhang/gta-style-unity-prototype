using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 连通校园大地图流程：考场→走廊→校门→公路城镇，同场景不跳转。
/// 被发现走 SafeDiscoveryReset；Transit/Travel 仍可走场景门。
/// </summary>
public class CampusFlowController : MonoBehaviour
{
    public Transform player;
    public Transform examCheckpoint;
    public Transform corridorCheckpoint;
    public Transform hubCheckpoint;
    public Transform homeCheckpoint;
    public Transform activeCheckpoint;
    public StealthGameHUD hud;
    public HubTownHUD hubHud;
    public WorldMapUI mapUi;

    public float failHoldSeconds = 0.7f;
    public float studyHoldSeconds = 0.8f;

    public enum Zone
    {
        Exam,
        School,
        Hub,
        Home
    }

    public Zone CurrentZone { get; private set; }
    public bool HasEscapedExam { get; private set; }
    public bool HasEscapedSchool { get; private set; }
    public bool IsBusy { get { return failing || studying; } }

    bool failing;
    bool studying;
    float bannerUntil;
    TeacherAgent[] agents;
    string secretTip;

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
        if (hubHud == null)
            hubHud = UnityCompat.FindOne<HubTownHUD>();
        if (mapUi == null)
            mapUi = UnityCompat.FindOne<WorldMapUI>();
        if (activeCheckpoint == null)
            activeCheckpoint = examCheckpoint;
        CurrentZone = Zone.Exam;
        GameProgress.Ensure().stage = GameProgress.StageExam;
        BindTeachers();
        SafeDiscoveryReset.BeginInvulnerability(3.5f);
        RefreshHud();
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

        // 家庭家长：若 VisionCone keepCheckingAfterSpot=false，也听发现事件
        VisionConeDetector[] cones = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] == null)
                continue;
            cones[i].OnPlayerSpotted -= HandleVisionSpot;
            cones[i].OnPlayerSpotted += HandleVisionSpot;
        }
    }

    void OnDestroy()
    {
        if (agents != null)
        {
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] != null)
                    agents[i].OnCaughtPlayer -= HandleCaught;
            }
        }
        VisionConeDetector[] cones = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] != null)
                cones[i].OnPlayerSpotted -= HandleVisionSpot;
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
    }

    void HandleCaught(TeacherAgent source, Transform spotted)
    {
        if (IsBusy || SafeDiscoveryReset.IsInvulnerable)
            return;
        if (CurrentZone == Zone.Hub)
            return;
        BeginCatchSequence(CurrentZone == Zone.Home ? "被家长发现了" : "被老师抓住了");
    }

    void HandleVisionSpot(VisionConeDetector source, Transform spotted)
    {
        if (IsBusy || SafeDiscoveryReset.IsInvulnerable)
            return;
        if (CurrentZone != Zone.Home)
            return;
        // 家长区：发现即回检查点
        BeginCatchSequence("被家长发现了");
    }

    void BeginCatchSequence(string title)
    {
        failing = true;
        bannerUntil = Time.time + failHoldSeconds;
        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.showSuccessBanner = false;
            hud.failTitle = title;
            hud.failSubtitle = "正在安全送回检查点…";
        }
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = false;
        }
        PauseThreats(true);
        ScrutinyAura[] auras = UnityCompat.FindAll<ScrutinyAura>();
        for (int i = 0; i < auras.Length; i++)
        {
            if (auras[i] != null)
                auras[i].SetForcePulse(true);
        }
    }

    void BeginForcedStudy()
    {
        failing = false;
        studying = true;
        bannerUntil = Time.time + studyHoldSeconds;

        Transform cp = activeCheckpoint != null ? activeCheckpoint : examCheckpoint;
        foreach (var teacher in UnityCompat.FindAll<TeacherAgent>()) teacher.ReturnHome();
        SafeDiscoveryReset.TryRespawn(player, cp);

        if (hud != null)
        {
            hud.showFailBanner = true;
            hud.failTitle = CurrentZone == Zone.Home ? "被送回屋内" : "被迫学习";
            hud.failSubtitle = "保持安全距离后短暂无敌，再继续逃离…";
        }
    }

    void FinishStudy()
    {
        studying = false;
        if (hud != null)
            hud.showFailBanner = false;
        PauseThreats(false);
        if (player != null)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null)
                motor.controlEnabled = true;
        }
        RefreshHud();
    }

    static void PauseThreats(bool paused)
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
        var examChapter=UnityCompat.FindOne<CampusExamChapter>();
        if (examChapter!=null && (!examChapter.SideOpen || player==null || player.position.x> -12.25f))return;
        if (HasEscapedExam || IsBusy)
            return;
        HasEscapedExam = true;
        if(examChapter!=null && !examChapter.State.corridorIntroduction)
        {
            examChapter.State.corridorIntroduction=true;
            examChapter.ShowEvent("corridor",CampusVoice.Inner,"原来还有这么长一条走廊……出口到底在哪里？先问问走廊里的同学。",7);
        }
        CurrentZone = Zone.School;
        activeCheckpoint = corridorCheckpoint != null ? corridorCheckpoint : activeCheckpoint;
        GameProgress.Ensure().stage = GameProgress.StageSchool;
        SaveSystem.Save();
        if (hud != null)
        {
            hud.showSuccessBanner = true;
            hud.successTitle = "逃出考场";
            hud.successSubtitle = UnityCompat.FindOne<CampusCorridorChapter>()!=null?"询问同学或查看公告，寻找 203 的备用通行卡":"继续沿走廊 / 楼梯 / 秘密通道前往校门（同地图，不跳转）";
            hud.showFailBanner = false;
        }
        RefreshHud();
        CancelInvoke("HideSuccess");
        Invoke("HideSuccess", 2.2f);
    }

    public void NotifySchoolEscaped()
    {
        var escape=UnityCompat.FindOne<CampusEscapeDirector>();
        if(escape!=null && !escape.GateOpen)return;
        if (HasEscapedSchool || IsBusy)return;
        var departure=UnityCompat.FindOne<CampusDepartureChapter>();
        if(departure!=null && !departure.TryDepart())return;
        HasEscapedSchool = true;
        CurrentZone = Zone.Hub;
        activeCheckpoint = hubCheckpoint != null ? hubCheckpoint : activeCheckpoint;
        GameProgress gp = GameProgress.Ensure();
        gp.MarkEscapedSchool();
        SaveSystem.Save();
        if (hud != null)
        {
            hud.showSuccessBanner = true;
            hud.successTitle = "逃出学校";
            hud.successSubtitle = "已进入校园外公路 / 城镇区（同地图）";
            hud.showFailBanner = false;
        }
        RefreshHud();
        CancelInvoke("HideSuccess");
        Invoke("HideSuccess", 2.2f);
    }

    public void RestoreChapterSession(Zone zone,Transform checkpoint)
    {
        failing=studying=false;HasEscapedExam=GameProgress.Ensure().examChapter.corridorIntroduction || zone!=Zone.Exam;HasEscapedSchool=GameProgress.Ensure().departureChapter.departed;
        CurrentZone=zone;activeCheckpoint=checkpoint;GameProgress.Ensure().stage=zone==Zone.Home?GameProgress.StageHome:zone==Zone.Hub?GameProgress.StageHub:zone==Zone.School?GameProgress.StageSchool:GameProgress.StageExam;
        PauseThreats(false);RefreshHud();
    }

    public void RestoreDeparture(bool departed)
    {
        HasEscapedSchool=departed;
        if(departed){HasEscapedExam=true;CurrentZone=Zone.Hub;activeCheckpoint=hubCheckpoint;}
        else if(CurrentZone==Zone.Hub){CurrentZone=Zone.School;activeCheckpoint=corridorCheckpoint;}
        RefreshHud();
    }

    public void SetZone(Zone zone)
    {
        if(zone==Zone.Hub && !HasEscapedSchool && UnityCompat.FindOne<CampusEscapeDirector>()!=null)return;
        if (zone == CurrentZone) return;
        if (zone == Zone.Hub)
        {
            var home = GameObject.Find("Zone_Home");
            if (home != null && player != null && home.GetComponent<Collider>().bounds.Contains(player.position)) return;
        }
        CurrentZone = zone;
        switch (zone)
        {
            case Zone.Exam:
                activeCheckpoint = examCheckpoint;
                break;
            case Zone.School:
                activeCheckpoint = corridorCheckpoint;
                break;
            case Zone.Hub:
                activeCheckpoint = hubCheckpoint;
                break;
            case Zone.Home:
                activeCheckpoint = homeCheckpoint;
                break;
        }
        RefreshHud();
    }

    public void RecoverFromFall()
    {
        failing = false;
        studying = false;
        foreach (var teacher in UnityCompat.FindAll<TeacherAgent>()) teacher.ReturnHome();
        PauseThreats(false);
        SafeDiscoveryReset.TryRespawn(player, activeCheckpoint != null ? activeCheckpoint : examCheckpoint);
        var motor = player != null ? player.GetComponent<ThirdPersonController>() : null;
        if (motor != null) motor.controlEnabled = true;
        RefreshHud();
        ShowTip("已返回检查点", "跌出地图后回到上一个出生点，短时间内不会被抓捕");
    }

    public void NotifyEnteredHome()
    {
        SetZone(Zone.Home);
        GameProgress.Ensure().stage = GameProgress.StageHome;
    }

    public void NotifyEnteredHub()
    {
        SetZone(Zone.Hub);
    }

    public void NotifySecretUsed(string tip)
    {
        secretTip = tip;
        if (hud != null)
        {
            hud.detail = "捷径：" + tip;
        }
    }

    void HideSuccess()
    {
        if (hud != null)
            hud.showSuccessBanner = false;
    }

    void RefreshHud()
    {
        if (hud == null)
            return;
        switch (CurrentZone)
        {
            case Zone.Exam:
                hud.objective = "潜行逃出二楼考场";
                hud.detail = "201 考场 · 避开红灯老师，前往左后方绿门或通风口";
                break;
            case Zone.School:
                hud.objective = "逃出学校";
                hud.detail = "沿二楼走廊前进 → 安全出口楼梯下至一楼 → 蓝色校门";
                break;
            case Zone.Hub:
                hud.objective = "现实城镇准备旅行";
                hud.detail = "买手机、赚钱、回家留条；蓝门可进枢纽（可跳场景）";
                break;
            case Zone.Home:
                hud.objective = "悄悄留下纸条";
                hud.detail = "冷紫灯光=家长；桌上按 E 留条";
                break;
        }
        if (!string.IsNullOrEmpty(secretTip))
            hud.detail = hud.detail + " · " + secretTip;
        hud.bottomPrompt = "WASD 移动  空格跳跃  Ctrl/C 蹲走  E 交互  M 地图  Esc 暂停";
        hud.showFailBanner = false;
    }

    public void ShowTip(string title, string sub)
    {
        if (hud != null)
        {
            hud.showSuccessBanner = true;
            hud.successTitle = title;
            hud.successSubtitle = sub;
            CancelInvoke("HideSuccess");
            Invoke("HideSuccess", 2.4f);
        }
        if (hubHud != null)
        {
            // HubTownHUD 可能有自己的 tip，忽略亦可
        }
    }

    /// <summary>可选：远端 Transit 仍 LoadScene。</summary>
    public void TryLoadScene(string sceneName)
    {
        var departure=UnityCompat.FindOne<CampusDepartureChapter>();
        if(departure!=null && !departure.State.completed){ShowTip("尚未离校",departure.Detail);return;}
        if (string.IsNullOrEmpty(sceneName))
            return;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            ShowTip("场景未就绪", "请用菜单生成并写入 Build Settings：" + sceneName);
            return;
        }
        SaveSystem.Save();
        SceneManager.LoadScene(sceneName);
    }
}

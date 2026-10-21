using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 《逃离考场》编辑器菜单：考场 / 学校 / 现实城镇 / 家庭 / 枢纽 / 旅行。
/// </summary>
public static class EscapeExamSceneMenu
{
    const string ExamScenePath = "Assets/Scenes/ExamHall.unity";
    const string SchoolScenePath = "Assets/Scenes/SchoolEscape.unity";
    const string HubTownScenePath = "Assets/Scenes/HubTown.unity";
    const string HomeScenePath = "Assets/Scenes/Home.unity";
    const string TransitScenePath = "Assets/Scenes/TransitHub.unity";
    const string TravelScenePath = "Assets/Scenes/TravelMissions.unity";
    const string OpenWorldScenePath = "Assets/Scenes/OpenWorld.unity";

    [MenuItem("EscapeExam/生成考场场景")]
    public static void GenerateExamHall()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(ExamScenePath, "b2000000000000000000000000000012");
        var scene = EditorSceneManager.OpenScene(ExamScenePath, OpenSceneMode.Single);
        ExamHallBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "考场场景已生成。\n点 Play：空格跳、蹲走躲老师追赶；被抓→座位被迫学习→再逃；绿门逃出。",
            "好");
    }

    [MenuItem("EscapeExam/生成学校场景")]
    public static void GenerateSchoolEscape()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(SchoolScenePath, "b2000000000000000000000000000022");
        var scene = EditorSceneManager.OpenScene(SchoolScenePath, OpenSceneMode.Single);
        SchoolEscapeBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "学校场景已生成。\n点 Play：多老师随机巡视追赶 → 蓝校门 → 进入现实城镇（非直接 TransitHub）。",
            "好");
    }

    [MenuItem("EscapeExam/生成现实城镇场景")]
    public static void GenerateHubTown()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(HubTownScenePath, "b400000000000000000000000000000e");
        var scene = EditorSceneManager.OpenScene(HubTownScenePath, OpenSceneMode.Single);
        HubTownBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "现实城镇已生成。\n回家留条 → 买手机 → 黄圈赚钱 → M 地图 → 蓝门进枢纽。",
            "好");
    }

    [MenuItem("EscapeExam/生成家庭场景")]
    public static void GenerateHome()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(HomeScenePath, "b400000000000000000000000000000a");
        var scene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
        HomeBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "家庭场景已生成。\n避开家长视野，桌上按 E 留条（自己赚钱出去旅游）。",
            "好");
    }

    [MenuItem("EscapeExam/生成交通枢纽场景")]
    public static void GenerateTransitHub()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(TransitScenePath, "b3000000000000000000000000000006");
        var scene = EditorSceneManager.OpenScene(TransitScenePath, OpenSceneMode.Single);
        TransitHubBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "交通枢纽已生成。\n需已留条；无手机看不到票价；有手机花钱购票 → 闸机 → 旅行。",
            "好");
    }

    [MenuItem("EscapeExam/生成旅行任务场景")]
    public static void GenerateTravelMissions()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(TravelScenePath, "b3000000000000000000000000000014");
        var scene = EditorSceneManager.OpenScene(TravelScenePath, OpenSceneMode.Single);
        TravelZoneBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "旅行任务场景已生成。\n点 Play：走进三个黄圈打卡并获得旅游资金。",
            "好");
    }

    [MenuItem("EscapeExam/打开考场场景")]
    public static void OpenExam() { OpenPath(ExamScenePath); }

    [MenuItem("EscapeExam/打开学校场景")]
    public static void OpenSchool() { OpenPath(SchoolScenePath); }

    [MenuItem("EscapeExam/打开现实城镇场景")]
    public static void OpenHubTown() { OpenPath(HubTownScenePath); }

    [MenuItem("EscapeExam/打开家庭场景")]
    public static void OpenHome() { OpenPath(HomeScenePath); }

    [MenuItem("EscapeExam/打开交通枢纽场景")]
    public static void OpenTransit() { OpenPath(TransitScenePath); }

    [MenuItem("EscapeExam/打开旅行任务场景")]
    public static void OpenTravel() { OpenPath(TravelScenePath); }

    [MenuItem("EscapeExam/将场景写入 Build Settings")]
    public static void MenuEnsureBuildSettings()
    {
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "已确保 OpenWorld / ExamHall / SchoolEscape / HubTown / Home / TransitHub / TravelMissions 在 Build Settings 中。",
            "好");
    }

    static bool GuardEditMode()
    {
        if (!EditorApplication.isPlaying)
            return true;
        EditorUtility.DisplayDialog("EscapeExam", "请先退出 Play 模式，再生成场景。", "好");
        return false;
    }

    static void OpenPath(string path)
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("EscapeExam", "请先退出 Play 模式。", "好");
            return;
        }

        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("EscapeExam", "找不到 " + path + "\n请先用「生成」菜单创建。", "好");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        EditorSceneManager.OpenScene(path);
    }

    static void EnsureSceneAssetWithBootstrap(string path, string bootstrapGuid)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(path))
            return;

        if (string.IsNullOrEmpty(bootstrapGuid))
            bootstrapGuid = "";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
    }

    static void EnsureBuildSettings()
    {
        var wanted = new[]
        {
            OpenWorldScenePath,
            ExamScenePath,
            SchoolScenePath,
            HubTownScenePath,
            HomeScenePath,
            TransitScenePath,
            TravelScenePath
        };

        var list = new List<EditorBuildSettingsScene>();
        var existing = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s == null || string.IsNullOrEmpty(s.path))
                continue;
            list.Add(s);
            existing.Add(s.path.Replace('\\', '/'));
        }

        foreach (string path in wanted)
        {
            string norm = path.Replace('\\', '/');
            if (!File.Exists(path))
                continue;
            if (existing.Contains(norm))
                continue;
            list.Add(new EditorBuildSettingsScene(norm, true));
            existing.Add(norm);
        }

        EditorBuildSettings.scenes = list.ToArray();
    }
}

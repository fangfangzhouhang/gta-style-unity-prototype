using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 《逃离考场》编辑器菜单：考场 / 学校 / 交通枢纽 / 旅行任务场景生成与 Build Settings。
/// </summary>
public static class EscapeExamSceneMenu
{
    const string ExamScenePath = "Assets/Scenes/ExamHall.unity";
    const string SchoolScenePath = "Assets/Scenes/SchoolEscape.unity";
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
            "考场场景已生成并保存。\n点 Play：蹲走躲监考 → 走到绿色出口 →「逃出考场」。",
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
            "学校场景已生成并保存。\n点 Play：避开巡逻 → 走到蓝色校门 →「逃出学校」→ 可进枢纽。",
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
            "交通枢纽已生成。\n点 Play：靠近橙色窗口按 E 选「G-自由01」→ 走过蓝色闸机 → 进入旅行区。",
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
            "旅行任务场景已生成。\n点 Play：走进三个黄圈（西湖 / 长城 / 黄山）完成打卡。",
            "好");
    }

    [MenuItem("EscapeExam/打开考场场景")]
    public static void OpenExam()
    {
        OpenPath(ExamScenePath);
    }

    [MenuItem("EscapeExam/打开学校场景")]
    public static void OpenSchool()
    {
        OpenPath(SchoolScenePath);
    }

    [MenuItem("EscapeExam/打开交通枢纽场景")]
    public static void OpenTransit()
    {
        OpenPath(TransitScenePath);
    }

    [MenuItem("EscapeExam/打开旅行任务场景")]
    public static void OpenTravel()
    {
        OpenPath(TravelScenePath);
    }

    [MenuItem("EscapeExam/将场景写入 Build Settings")]
    public static void MenuEnsureBuildSettings()
    {
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "已确保 OpenWorld / ExamHall / SchoolEscape / TransitHub / TravelMissions 在 Build Settings 中。",
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

        // bootstrapGuid 预留给将来写入带组件的 YAML；目前仓库已带 Bootstrap 场景文件。
        if (string.IsNullOrEmpty(bootstrapGuid))
            bootstrapGuid = "";

        // 仓库已提供带 Bootstrap 的 YAML 场景；若缺失则建空场景，生成菜单会挂上 Builder。
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

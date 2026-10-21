using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 《逃离考场》v0.2：生成考场 / 学校可玩占位场景，并写入 Build Settings。
/// </summary>
public static class EscapeExamSceneMenu
{
    const string ExamScenePath = "Assets/Scenes/ExamHall.unity";
    const string SchoolScenePath = "Assets/Scenes/SchoolEscape.unity";
    const string OpenWorldScenePath = "Assets/Scenes/OpenWorld.unity";

    [MenuItem("EscapeExam/生成考场场景")]
    public static void GenerateExamHall()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("EscapeExam", "请先退出 Play 模式，再生成场景。", "好");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAsset(ExamScenePath);
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
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("EscapeExam", "请先退出 Play 模式，再生成场景。", "好");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAsset(SchoolScenePath);
        var scene = EditorSceneManager.OpenScene(SchoolScenePath, OpenSceneMode.Single);
        SchoolEscapeBuilder.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "学校场景已生成并保存。\n点 Play：避开巡逻 → 走到蓝色校门 →「逃出学校」。",
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

    [MenuItem("EscapeExam/将场景写入 Build Settings")]
    public static void MenuEnsureBuildSettings()
    {
        EnsureBuildSettings();
        EditorUtility.DisplayDialog("EscapeExam", "已确保 OpenWorld / ExamHall / SchoolEscape 在 Build Settings 中。", "好");
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

    static void EnsureSceneAsset(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(path))
            return;

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
            SchoolScenePath
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

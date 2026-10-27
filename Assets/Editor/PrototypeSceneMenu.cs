using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 打开工程时如果还是未保存的空场景，就切到原型场景。
/// 菜单可以按脚本把占位街区重新摆一遍。
/// </summary>
[InitializeOnLoad]
public static class PrototypeSceneMenu
{
    const string ScenePath = "Assets/Scenes/OpenWorld.unity";

    static PrototypeSceneMenu()
    {
        EditorApplication.delayCall += OpenPrototypeIfUntitled;
    }

    static void OpenPrototypeIfUntitled()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        var active = EditorSceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(active.path))
            return;
        if (!System.IO.File.Exists(ScenePath))
            return;

        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("OpenWorld/打开原型场景")]
    public static void OpenScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("OpenWorld", "请先退出 Play 模式。", "好");
            return;
        }

        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("OpenWorld/生成占位场景")]
    public static void Generate()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("OpenWorld", "请先退出 Play 模式，再生成场景。", "好");
            return;
        }

        if (!System.IO.File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("OpenWorld", "找不到 " + ScenePath, "好");
            return;
        }

        var active = EditorSceneManager.GetActiveScene();
        if (active.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        PrototypeWorldBuilder.Build();
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("OpenWorld", "占位街区已重新生成并保存。直接点 Play 即可验收。", "好");
    }
}

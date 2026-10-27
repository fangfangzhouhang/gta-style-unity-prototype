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
    const string NetLobbyScenePath = "Assets/Scenes/NetLobby.unity";
    const string NetExamScenePath = "Assets/Scenes/NetExamHall.unity";
    const string NetMeetupScenePath = "Assets/Scenes/NetMeetup.unity";
    const string CampusWorldScenePath = "Assets/Scenes/CampusWorld.unity";


    [MenuItem("EscapeExam/生成连通校园大地图 (CampusWorld)")]
    public static void GenerateCampusWorld()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(CampusWorldScenePath, "b6000000000000000000000000000006");
        var scene = EditorSceneManager.OpenScene(CampusWorldScenePath, OpenSceneMode.Single);
        CampusAssetBaker.PrepareTextures();
        CampusWorldBuilder.Build();
        CampusAssetBaker.Save(GameObject.Find("LevelRoot").transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "CampusWorld 连通大地图已生成并写入场景。\n" +
            "点 Play：考场→走廊/楼梯/秘密通道→校门→公路城镇（同场景不跳转）。\n" +
            "老师冷红灯 / 家长冷紫灯；被发现安全回位。\n" +
            "枢纽/旅行仍可走场景门。",
            "好");
    }

    [MenuItem("EscapeExam/打开连通校园大地图")]
    public static void OpenCampusWorld() { OpenPath(CampusWorldScenePath); }

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


    [MenuItem("EscapeExam/生成联机大厅/联机考场骨架")]
    public static void GenerateNetcodeSkeleton()
    {
        if (!GuardEditMode())
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneAssetWithBootstrap(NetLobbyScenePath, "b5000000000000000000000000000008");
        EnsureSceneAssetWithBootstrap(NetExamScenePath, "b5000000000000000000000000000009");
        EnsureSceneAssetWithBootstrap(NetMeetupScenePath, "b500000000000000000000000000000b");

        var lobby = EditorSceneManager.OpenScene(NetLobbyScenePath, OpenSceneMode.Single);
        // Clear and place lobby bootstrap
        foreach (var root in lobby.GetRootGameObjects())
            UnityEngine.Object.DestroyImmediate(root);
        var lobbyGo = new GameObject("NetLobbyBootstrap");
        lobbyGo.AddComponent<NetLobbyBootstrap>();
        EditorSceneManager.MarkSceneDirty(lobby);
        EditorSceneManager.SaveScene(lobby);

        var exam = EditorSceneManager.OpenScene(NetExamScenePath, OpenSceneMode.Single);
        foreach (var root in exam.GetRootGameObjects())
            UnityEngine.Object.DestroyImmediate(root);
        var examGo = new GameObject("NetExamHallBootstrap");
        examGo.AddComponent<NetExamHallBootstrap>();
        NetExamHallBuilder.Build();
        EditorSceneManager.MarkSceneDirty(exam);
        EditorSceneManager.SaveScene(exam);

        var meetup = EditorSceneManager.OpenScene(NetMeetupScenePath, OpenSceneMode.Single);
        foreach (var root in meetup.GetRootGameObjects())
            UnityEngine.Object.DestroyImmediate(root);
        var meetGo = new GameObject("NetMeetupBootstrap");
        meetGo.AddComponent<NetMeetupBootstrap>();
        NetMeetupBuilder.Build();
        EditorSceneManager.MarkSceneDirty(meetup);
        EditorSceneManager.SaveScene(meetup);

        // Return to lobby for Host/Client UI
        EditorSceneManager.OpenScene(NetLobbyScenePath, OpenSceneMode.Single);
        EnsureNetworkPlayerPrefabAsset();
        EnsureBuildSettings();
        EditorUtility.DisplayDialog(
            "EscapeExam",
            "联机骨架已生成（NetLobby / NetExamHall / NetMeetup）。\n" +
            "首次打开请等 Package Manager 解析 com.unity.netcode.gameobjects。\n" +
            "本机测：Build 两份或编辑器 Play Host + 另一进程 Client（127.0.0.1:7777）。\n" +
            "这是骨架，不是完整联机产品（无 Relay / 匹配 / 语音）。",
            "好");
    }

    [MenuItem("EscapeExam/打开联机大厅")]
    public static void OpenNetLobby() { OpenPath(NetLobbyScenePath); }

    [MenuItem("EscapeExam/打开联机考场")]
    public static void OpenNetExam() { OpenPath(NetExamScenePath); }

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
            "已确保 CampusWorld（默认第0）/ OpenWorld / ExamHall / SchoolEscape / HubTown / Home / TransitHub / TravelMissions / Net* 在 Build Settings 中。",
            "好");
    }


    static void EnsureNetworkPlayerPrefabAsset()
    {
        const string prefabPath = "Assets/Resources/NetworkPlayer.prefab";
        string dir = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            // Resources usually exists; create if needed
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        GameObject template = NetPlayerTemplateFactory.EnsureTemplate();
        template.SetActive(true);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(template, prefabPath);
        template.SetActive(false);
        if (prefab != null)
            Debug.Log("[EscapeExam] NetworkPlayer prefab → " + prefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
            CampusWorldScenePath,
            OpenWorldScenePath,
            ExamScenePath,
            SchoolScenePath,
            HubTownScenePath,
            HomeScenePath,
            TransitScenePath,
            TravelScenePath,
            NetLobbyScenePath,
            NetExamScenePath,
            NetMeetupScenePath
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


        // 保证 CampusWorld 为 Build Settings 第 0 项（默认 Play 入口）
        int campusIdx = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].path.Replace('\\', '/') == CampusWorldScenePath)
            {
                campusIdx = i;
                break;
            }
        }
        if (campusIdx > 0)
        {
            var campus = list[campusIdx];
            list.RemoveAt(campusIdx);
            list.Insert(0, campus);
        }
        EditorBuildSettings.scenes = list.ToArray();
    }
}

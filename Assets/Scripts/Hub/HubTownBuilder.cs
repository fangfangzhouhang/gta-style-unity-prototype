using UnityEngine;

/// <summary>
/// 逃出学校后的现实城镇占位：手机摊、赚钱点、家门、枢纽门、地图。
/// </summary>
public static class HubTownBuilder
{
    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();
        var root = new GameObject("LevelRoot").transform;

        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Ground",
            new Vector3(0f, -0.1f, 20f), new Vector3(48f, 0.2f, 56f), Quaternion.identity,
            EscapePalette.TravelGround, "Sidewalk", true);

        // 简楼占位
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "BldgA",
            new Vector3(-14f, 3f, 8f), new Vector3(6f, 6f, 6f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "BldgB",
            new Vector3(14f, 2.5f, 18f), new Vector3(5f, 5f, 5f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);

        // 手机路边摊
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "PhoneStall",
            new Vector3(-6f, 0.6f, 6f), new Vector3(2.4f, 1.2f, 1.4f), Quaternion.identity,
            EscapePalette.TicketWindow, "Crate", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "PhoneSign",
            new Vector3(-6f, 1.6f, 6.6f), new Vector3(1.6f, 0.5f, 0.12f), Quaternion.identity,
            EscapePalette.TicketScreen, "Glass", false, true, 0.35f);
        var phone = new GameObject("BuyPhone");
        phone.transform.SetParent(root, false);
        phone.transform.position = new Vector3(-6f, 1f, 4.8f);
        var phoneCol = phone.AddComponent<BoxCollider>();
        phoneCol.isTrigger = true;
        phoneCol.size = new Vector3(3f, 2f, 3f);
        phone.AddComponent<BuyPhoneInteract>();

        // 赚钱任务点
        CreateEarn(root, "Earn_Post", new Vector3(8f, 0f, 10f), "送信跑腿", 40);
        CreateEarn(root, "Earn_Clean", new Vector3(-10f, 0f, 22f), "打扫广场", 35);
        CreateEarn(root, "Earn_Help", new Vector3(4f, 0f, 28f), "帮摊位搬货", 45);

        // 家门（绿）
        CreatePortal(root, "PortalHome", new Vector3(-12f, 0f, 36f), EscapePalette.Exit,
            "Home", "按 E 回家留纸条", false, false, false);

        // 枢纽门（蓝）—— 需要留条；手机在购票窗口再查
        CreatePortal(root, "PortalTransit", new Vector3(12f, 0f, 36f), EscapePalette.Gate,
            "TransitHub", "按 E 进入交通枢纽（需已留条）", false, true, false);

        Vector3 spawn = new Vector3(0f, 1.02f, 0f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);

        Wire(player, cam);
    }

    static void CreateEarn(Transform root, string name, Vector3 pos, string title, int reward)
    {
        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, name + "_Visual",
            pos + new Vector3(0f, 1.2f, 0f), new Vector3(2.2f, 1.2f, 2.2f), Quaternion.identity,
            new Color(1f, 0.85f, 0.15f, 1f), "Checkpoint", false, true, 0.45f);

        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        var point = go.AddComponent<EarnMoneyPoint>();
        point.title = title;
        point.reward = reward;
        point.radius = 2.4f;
    }

    static void CreatePortal(Transform root, string name, Vector3 pos, Color color,
        string scene, string prompt, bool needSchool, bool needNote, bool needPhone)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Visual",
            pos + new Vector3(0f, 1.5f, 0f), new Vector3(2.4f, 3f, 0.4f), Quaternion.identity,
            color, "Glass", false, true, 0.4f);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Pad",
            pos + new Vector3(0f, -0.02f, 1.2f), new Vector3(4f, 0.08f, 3f), Quaternion.identity,
            color, "Glass", true, true, 0.25f);

        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos + new Vector3(0f, 1f, 0.8f);
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.5f, 2.5f, 3f);
        var portal = go.AddComponent<ScenePortalInteract>();
        portal.targetScene = scene;
        portal.promptNear = prompt;
        portal.requireEscapedSchool = needSchool;
        portal.requireLeftNote = needNote;
        portal.requirePhone = needPhone;
    }

    static void Wire(GameObject player, Camera cam)
    {
        var motor = player.GetComponent<ThirdPersonController>();
        var follow = cam.GetComponent<ThirdPersonCamera>();
        motor.view = cam.transform;
        motor.allowCrouch = true;
        follow.target = player.transform;
        follow.targetOffset = ThirdPersonCamera.OnFootOffset;
        follow.distance = ThirdPersonCamera.OnFootDistance;

        GameObject systems = GameObject.Find("Game");
        if (systems == null)
            systems = new GameObject("Game");
        if (systems.GetComponent<HubTownBootstrap>() == null)
            systems.AddComponent<HubTownBootstrap>();

        HubTownHUD hud = systems.GetComponent<HubTownHUD>();
        if (hud == null)
            hud = systems.AddComponent<HubTownHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        WorldMapUI map = systems.GetComponent<WorldMapUI>();
        if (map == null)
            map = systems.AddComponent<WorldMapUI>();
        if (map.uiFont == null)
            map.uiFont = Resources.Load<Font>("UIFont");

        HubTownFlowController flow = systems.GetComponent<HubTownFlowController>();
        if (flow == null)
            flow = systems.AddComponent<HubTownFlowController>();
        flow.hud = hud;
        flow.mapUi = map;
        flow.player = player.transform;

        EarnMoneyPoint[] earns = UnityCompat.FindAll<EarnMoneyPoint>();
        for (int i = 0; i < earns.Length; i++)
        {
            if (earns[i] != null)
                earns[i].flow = flow;
        }
        ScenePortalInteract[] portals = UnityCompat.FindAll<ScenePortalInteract>();
        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null)
                portals[i].flow = flow;
        }

        PauseMenu.Ensure();
        follow.Snap();
    }
}

using UnityEngine;

/// <summary>
/// 生成综合交通枢纽占位：高铁窗口 / 地铁闸机 / 机场值机（同厅）。
/// </summary>
public static class TransitHubBuilder
{
    public static void Build()
    {
        // 进入枢纽时清票，强制走一遍购票流程
        TicketInventory.Clear();

        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("LevelRoot").transform;

        // 大厅地面
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "HallFloor",
            new Vector3(0f, -0.1f, 12f), new Vector3(28f, 0.2f, 36f), Quaternion.identity,
            EscapePalette.HubFloor, "Sidewalk", true);

        // 外墙矮隔断
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallBack",
            new Vector3(0f, 2f, -5.5f), new Vector3(28f, 4f, 0.4f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallL",
            new Vector3(-14.2f, 2f, 12f), new Vector3(0.4f, 4f, 36f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallR",
            new Vector3(14.2f, 2f, 12f), new Vector3(0.4f, 4f, 36f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        // —— 高铁购票窗口（左侧橙色）——
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "TicketCounter",
            new Vector3(-8f, 0.7f, 4f), new Vector3(6f, 1.4f, 1.2f), Quaternion.identity,
            EscapePalette.TicketWindow, "Crate", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "TicketScreen",
            new Vector3(-8f, 1.8f, 4.55f), new Vector3(4.5f, 1.2f, 0.15f), Quaternion.identity,
            EscapePalette.TicketScreen, "Glass", false, true, 0.4f);

        var window = new GameObject("TicketWindow");
        window.transform.SetParent(root, false);
        window.transform.position = new Vector3(-8f, 1f, 2.2f);
        var winCol = window.AddComponent<BoxCollider>();
        winCol.isTrigger = true;
        winCol.size = new Vector3(5f, 2.2f, 3.2f);
        window.AddComponent<TicketWindowInteract>();

        // —— 机场值机占位（右侧青蓝，视觉 + 也可购票）——
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "CheckInDesk",
            new Vector3(8f, 0.7f, 4f), new Vector3(6f, 1.4f, 1.2f), Quaternion.identity,
            EscapePalette.AirportDesk, "BuildingB", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "CheckInSign",
            new Vector3(8f, 2.2f, 4.5f), new Vector3(3.5f, 0.8f, 0.2f), Quaternion.identity,
            EscapePalette.Gate, "Glass", false, true, 0.35f);

        var checkIn = new GameObject("AirportCheckIn");
        checkIn.transform.SetParent(root, false);
        checkIn.transform.position = new Vector3(8f, 1f, 2.2f);
        var ciCol = checkIn.AddComponent<BoxCollider>();
        ciCol.isTrigger = true;
        ciCol.size = new Vector3(5f, 2.2f, 3.2f);
        var ciInteract = checkIn.AddComponent<TicketWindowInteract>();
        ciInteract.promptNear = "按 E 值机选班次（机场占位）";

        // —— 地铁闸机（深处蓝色）——
        CreateGateVisual(root, new Vector3(-2.2f, 1f, 22f));
        CreateGateVisual(root, new Vector3(2.2f, 1f, 22f));
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "GatePad",
            new Vector3(0f, -0.02f, 23.5f), new Vector3(8f, 0.08f, 5f), Quaternion.identity,
            EscapePalette.Gate, "Glass", true, true, 0.3f);

        var gate = new GameObject("SubwayGate");
        gate.transform.SetParent(root, false);
        gate.transform.position = new Vector3(0f, 1f, 22.5f);
        var gateCol = gate.AddComponent<BoxCollider>();
        gateCol.isTrigger = true;
        gateCol.size = new Vector3(5.5f, 2.4f, 3f);
        gate.AddComponent<TicketGateInteract>();

        // 导向地标柱
        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, "PillarA",
            new Vector3(-4f, 2f, 12f), new Vector3(0.6f, 4f, 0.6f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, "PillarB",
            new Vector3(4f, 2f, 12f), new Vector3(0.6f, 4f, 0.6f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        Vector3 spawn = new Vector3(0f, 1.02f, -2f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, false);

        Wire(player, cam);
    }

    static void CreateGateVisual(Transform root, Vector3 pos)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "GatePost",
            pos, new Vector3(0.5f, 2f, 0.8f), Quaternion.identity,
            EscapePalette.Gate, "Glass", true, true, 0.45f);
    }

    static void Wire(GameObject player, Camera cam)
    {
        var motor = player.GetComponent<ThirdPersonController>();
        var follow = cam.GetComponent<ThirdPersonCamera>();
        motor.view = cam.transform;
        follow.target = player.transform;
        follow.targetOffset = ThirdPersonCamera.OnFootOffset;
        follow.distance = ThirdPersonCamera.OnFootDistance;

        GameObject systems = GameObject.Find("Game");
        if (systems == null)
            systems = new GameObject("Game");

        if (systems.GetComponent<TransitHubBootstrap>() == null)
            systems.AddComponent<TransitHubBootstrap>();

        TransitGameHUD hud = systems.GetComponent<TransitGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<TransitGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        TransitFlowController flow = systems.GetComponent<TransitFlowController>();
        if (flow == null)
            flow = systems.AddComponent<TransitFlowController>();
        flow.player = player.transform;
        flow.hud = hud;
        hud.flow = flow;

        TicketWindowInteract[] windows = UnityCompat.FindAll<TicketWindowInteract>();
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null)
                windows[i].flow = flow;
        }

        TicketGateInteract gate = UnityCompat.FindOne<TicketGateInteract>();
        if (gate != null)
            gate.flow = flow;

        // 去掉潜行/任务旧 HUD，避免叠字
        StealthGameHUD stealth = systems.GetComponent<StealthGameHUD>();
        if (stealth != null)
            LevelBuildUtil.DestroyObject(stealth);
        MissionHUD legacy = systems.GetComponent<MissionHUD>();
        if (legacy != null)
            LevelBuildUtil.DestroyObject(legacy);

        flow.RefreshHudIdle();
        PauseMenu.Ensure();
        GameProgress.Ensure();
        follow.Snap();
    }
}

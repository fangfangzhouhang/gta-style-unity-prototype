using UnityEngine;

/// <summary>
/// 生成家庭潜行占位：客厅、桌子、家长视野巡逻、留条交互。
/// </summary>
public static class HomeBuilder
{
    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();
        var root = new GameObject("LevelRoot").transform;

        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Floor",
            new Vector3(0f, -0.1f, 6f), new Vector3(16f, 0.2f, 18f), Quaternion.identity,
            EscapePalette.HomeFloor, "Sidewalk", true);

        MakeWall(root, "WallBack", new Vector3(0f, 1.8f, 14.8f), new Vector3(16f, 3.6f, 0.35f));
        MakeWall(root, "WallFront", new Vector3(0f, 1.8f, -2.8f), new Vector3(16f, 3.6f, 0.35f));
        MakeWall(root, "WallL", new Vector3(-8f, 1.8f, 6f), new Vector3(0.35f, 3.6f, 18f));
        MakeWall(root, "WallR", new Vector3(8f, 1.8f, 6f), new Vector3(0.35f, 3.6f, 18f));

        // 桌子（留条点）
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Table",
            new Vector3(0f, 0.5f, 10f), new Vector3(2.2f, 0.2f, 1.2f), Quaternion.identity,
            EscapePalette.Desk, "Crate", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "NotePaper",
            new Vector3(0f, 0.65f, 10f), new Vector3(0.5f, 0.04f, 0.35f), Quaternion.identity,
            Color.white, "Glass", false, true, 0.2f);

        var note = new GameObject("LeaveNote");
        note.transform.SetParent(root, false);
        note.transform.position = new Vector3(0f, 1f, 9.2f);
        var noteCol = note.AddComponent<BoxCollider>();
        noteCol.isTrigger = true;
        noteCol.size = new Vector3(2.5f, 2f, 2f);
        note.AddComponent<LeaveNoteInteract>();

        // 沙发掩体
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Sofa",
            new Vector3(-4.5f, 0.55f, 4f), new Vector3(2.8f, 1.1f, 1.2f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true).AddComponent<CoverTag>().label = "沙发";
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Shelf",
            new Vector3(5.5f, 1.1f, 8f), new Vector3(1.2f, 2.2f, 0.6f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true).AddComponent<CoverTag>().label = "柜子";

        CreateParent(root);

        Vector3 spawn = new Vector3(0f, 1.02f, -0.5f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);

        var checkpoint = new GameObject("Checkpoint");
        checkpoint.transform.SetParent(root, false);
        checkpoint.transform.position = spawn;

        Wire(player, cam, checkpoint.transform);
    }

    static void MakeWall(Transform root, string name, Vector3 pos, Vector3 scale)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name, pos, scale, Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
    }

    static void CreateParent(Transform root)
    {
        var go = new GameObject("Parent");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(3f, 0f, 6f);

        LevelBuildUtil.Make(PrimitiveType.Capsule, go.transform, "Visual",
            new Vector3(0f, 1f, 0f), new Vector3(0.85f, 1f, 0.85f), Quaternion.identity,
            EscapePalette.Parent, "CarBody", false);

        var col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1f, 0f);
        col.height = 2f;
        col.radius = 0.4f;

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = 8.5f;
        cone.viewAngle = 60f;
        cone.eyeHeight = 1.55f;
        cone.keepCheckingAfterSpot = false;

        var wpA = new GameObject("Parent_A");
        wpA.transform.SetParent(root, false);
        wpA.transform.position = new Vector3(3f, 0f, 3f);
        var wpB = new GameObject("Parent_B");
        wpB.transform.SetParent(root, false);
        wpB.transform.position = new Vector3(3f, 0f, 11f);
        var wpC = new GameObject("Parent_C");
        wpC.transform.SetParent(root, false);
        wpC.transform.position = new Vector3(-2f, 0f, 7f);

        var agent = go.AddComponent<TeacherAgent>();
        agent.waypoints = new[] { wpA.transform, wpB.transform, wpC.transform };
        agent.patrolSpeed = 1.7f;
        agent.chaseSpeed = 3.8f;
        agent.randomPatrol = true;
        var aura = go.AddComponent<ScrutinyAura>();
        aura.SetRole(ScrutinyAura.Role.Parent);
        // 家长抓住也走视野事件：HomeFlow 直接听 VisionCone；追赶仅为压迫感
        // 发现即失败由 keepCheckingAfterSpot=false + HomeFlow 订阅实现
    }

    static void Wire(GameObject player, Camera cam, Transform checkpoint)
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
        if (systems.GetComponent<HomeBootstrap>() == null)
            systems.AddComponent<HomeBootstrap>();

        StealthGameHUD hud = systems.GetComponent<StealthGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<StealthGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        HomeFlowController flow = systems.GetComponent<HomeFlowController>();
        if (flow == null)
            flow = systems.AddComponent<HomeFlowController>();
        flow.checkpoint = checkpoint;
        flow.player = player.transform;
        flow.hud = hud;

        LeaveNoteInteract note = UnityCompat.FindOne<LeaveNoteInteract>();
        if (note != null)
            note.flow = flow;

        PauseMenu.Ensure();
        flow.BindDetectors();
        follow.Snap();
    }
}

using UnityEngine;

/// <summary>
/// 生成学校走廊占位关：走廊、柜子掩体、1–2 名巡逻、蓝色校门。
/// </summary>
public static class SchoolEscapeBuilder
{
    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("LevelRoot").transform;

        // 走廊地面（沿 +Z）
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "CorridorFloor",
            new Vector3(0f, -0.1f, 20f), new Vector3(8f, 0.2f, 48f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);

        // 两侧墙
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallL",
            new Vector3(-4.2f, 1.8f, 20f), new Vector3(0.4f, 3.6f, 48f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallR",
            new Vector3(4.2f, 1.8f, 20f), new Vector3(0.4f, 3.6f, 48f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        // 起点后墙
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "WallStart",
            new Vector3(0f, 1.8f, -4.2f), new Vector3(8.8f, 3.6f, 0.4f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        // 柜子掩体
        CreateCabinet(root, new Vector3(-3.2f, 1f, 6f));
        CreateCabinet(root, new Vector3(3.2f, 1f, 12f));
        CreateCabinet(root, new Vector3(-3.2f, 1f, 18f));
        CreateCabinet(root, new Vector3(3.2f, 1f, 24f));
        CreateCabinet(root, new Vector3(-3.2f, 1f, 30f));

        // 巡逻 1：走廊中段
        CreatePatrol(root, "Patrol_A",
            new Vector3(1.5f, 0f, 10f),
            new Vector3(1.5f, 0f, 8f),
            new Vector3(1.5f, 0f, 22f),
            2.0f);

        // 巡逻 2：靠校门
        CreatePatrol(root, "Patrol_B",
            new Vector3(-1.2f, 0f, 28f),
            new Vector3(-1.2f, 0f, 26f),
            new Vector3(-1.2f, 0f, 36f),
            1.8f);

        Vector3 spawn = new Vector3(0f, 1.02f, -1.5f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);

        var checkpoint = new GameObject("Checkpoint");
        checkpoint.transform.SetParent(root, false);
        checkpoint.transform.position = spawn;

        // 校门（走廊尽头）
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "GateVisual",
            new Vector3(0f, 1.6f, 42f), new Vector3(3.2f, 3.2f, 0.35f), Quaternion.identity,
            EscapePalette.Gate, "Glass", false, true, 0.5f);

        LevelBuildUtil.Make(PrimitiveType.Cube, root, "GatePad",
            new Vector3(0f, -0.02f, 43.5f), new Vector3(6f, 0.08f, 4f), Quaternion.identity,
            EscapePalette.Gate, "Glass", true, true, 0.35f);

        var gate = new GameObject("SchoolGate");
        gate.transform.SetParent(root, false);
        gate.transform.position = new Vector3(0f, 1f, 42.5f);
        var box = gate.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(4f, 2.5f, 2.5f);
        gate.AddComponent<SchoolGateTrigger>();

        Wire(player, cam, checkpoint.transform);
    }

    static void CreateCabinet(Transform root, Vector3 pos)
    {
        var cab = LevelBuildUtil.Make(PrimitiveType.Cube, root, "Cabinet",
            pos, new Vector3(1.0f, 2.0f, 0.8f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);
        cab.AddComponent<CoverTag>().label = "柜子";
    }

    static void CreatePatrol(Transform root, string name, Vector3 start, Vector3 a, Vector3 b, float speed)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = start;

        LevelBuildUtil.Make(PrimitiveType.Capsule, go.transform, "Visual",
            new Vector3(0f, 1f, 0f), new Vector3(0.85f, 1f, 0.85f), Quaternion.identity,
            EscapePalette.Patrol, "BuildingC", false);

        var col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1f, 0f);
        col.height = 2f;
        col.radius = 0.4f;

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = 9f;
        cone.viewAngle = 60f;
        cone.eyeHeight = 1.55f;

        var wpA = new GameObject(name + "_A");
        wpA.transform.SetParent(root, false);
        wpA.transform.position = a;
        var wpB = new GameObject(name + "_B");
        wpB.transform.SetParent(root, false);
        wpB.transform.position = b;

        var patrol = go.AddComponent<SimplePatrol>();
        patrol.waypoints = new[] { wpA.transform, wpB.transform };
        patrol.moveSpeed = speed;
        patrol.waitAtPoint = 0.6f;
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

        if (systems.GetComponent<SchoolEscapeBootstrap>() == null)
            systems.AddComponent<SchoolEscapeBootstrap>();

        StealthGameHUD hud = systems.GetComponent<StealthGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<StealthGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        SchoolFlowController flow = systems.GetComponent<SchoolFlowController>();
        if (flow == null)
            flow = systems.AddComponent<SchoolFlowController>();
        flow.checkpoint = checkpoint;
        flow.player = player.transform;
        flow.hud = hud;

        SchoolGateTrigger gate = UnityCompat.FindOne<SchoolGateTrigger>();
        if (gate != null)
            gate.flow = flow;

        flow.BindDetectors();
        follow.Snap();

        MissionHUD legacy = systems.GetComponent<MissionHUD>();
        if (legacy != null)
            LevelBuildUtil.DestroyObject(legacy);
    }
}

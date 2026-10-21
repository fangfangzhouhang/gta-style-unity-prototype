using UnityEngine;

/// <summary>
/// 生成可玩考场占位关：地面、墙、课桌掩体、监考视野、检查点、绿色出口。
/// </summary>
public static class ExamHallBuilder
{
    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("LevelRoot").transform;

        // 地面
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Floor",
            new Vector3(0f, -0.1f, 8f), new Vector3(22f, 0.2f, 28f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);

        // 四面墙
        MakeWall(root, "WallBack", new Vector3(0f, 1.8f, -5.5f), new Vector3(22f, 3.6f, 0.4f));
        MakeWall(root, "WallFront", new Vector3(0f, 1.8f, 21.5f), new Vector3(22f, 3.6f, 0.4f));
        MakeWall(root, "WallLeft", new Vector3(-11f, 1.8f, 8f), new Vector3(0.4f, 3.6f, 27.4f));
        MakeWall(root, "WallRight", new Vector3(11f, 1.8f, 8f), new Vector3(0.4f, 3.6f, 27.4f));

        // 黑板
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Blackboard",
            new Vector3(0f, 2.2f, 21.1f), new Vector3(8f, 2.2f, 0.12f), Quaternion.identity,
            EscapePalette.Blackboard, "BuildingC", false);

        // 讲台
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Podium",
            new Vector3(0f, 0.45f, 18.5f), new Vector3(2.4f, 0.9f, 1.2f), Quaternion.identity,
            EscapePalette.Desk, "Crate", true);

        CreateDesks(root);

        // 侧边柜子掩体（通往出口）
        CreateCabinet(root, new Vector3(-9.2f, 1.0f, 4f));
        CreateCabinet(root, new Vector3(-9.2f, 1.0f, 8f));
        CreateCabinet(root, new Vector3(9.2f, 1.0f, 12f));

        // 讲台主监考：多路点随机巡视（考场前区）
        Transform[] frontRoute =
        {
            MakeWp(root, "T1_A", new Vector3(-3f, 0f, 16.5f)),
            MakeWp(root, "T1_B", new Vector3(3f, 0f, 16.5f)),
            MakeWp(root, "T1_C", new Vector3(0f, 0f, 18.5f)),
            MakeWp(root, "T1_D", new Vector3(0f, 0f, 14.5f))
        };
        CreateTeacher(root, new Vector3(0f, 0f, 17.2f), Quaternion.Euler(0f, 180f, 0f),
            frontRoute, 1.5f, 4.4f);

        // 侧边监考：走廊式随机路点
        Transform[] sideRoute =
        {
            MakeWp(root, "T2_A", new Vector3(8.5f, 0f, 4f)),
            MakeWp(root, "T2_B", new Vector3(8.5f, 0f, 10f)),
            MakeWp(root, "T2_C", new Vector3(8.5f, 0f, 16f)),
            MakeWp(root, "T2_D", new Vector3(6.5f, 0f, 12f))
        };
        GameObject sideProctor = CreateTeacher(root, new Vector3(8.5f, 0f, 10f),
            Quaternion.Euler(0f, -90f, 0f), sideRoute, 1.8f, 4.8f);
        sideProctor.name = "SideProctor";
        VisionConeDetector sideCone = sideProctor.GetComponent<VisionConeDetector>();
        if (sideCone != null)
        {
            sideCone.viewDistance = 8.5f;
            sideCone.viewAngle = 55f;
        }

        // 后排巡视老师（考场深处）
        Transform[] rearRoute =
        {
            MakeWp(root, "T3_A", new Vector3(-7f, 0f, 2f)),
            MakeWp(root, "T3_B", new Vector3(0f, 0f, 0.5f)),
            MakeWp(root, "T3_C", new Vector3(7f, 0f, 2f)),
            MakeWp(root, "T3_D", new Vector3(0f, 0f, 4f))
        };
        GameObject rear = CreateTeacher(root, new Vector3(-7f, 0f, 2f),
            Quaternion.Euler(0f, 90f, 0f), rearRoute, 1.7f, 4.5f);
        rear.name = "RearProctor";

        // 玩家出生：后排座位附近（检查点）
        Vector3 spawn = new Vector3(0f, 1.02f, -2.5f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);

        var checkpoint = new GameObject("Checkpoint");
        checkpoint.transform.SetParent(root, false);
        checkpoint.transform.position = spawn;
        checkpoint.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // 出口：左侧靠后墙缺口旁的绿色门
        // 先在左墙开个视觉缺口（矮门框）
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "ExitDoor",
            new Vector3(-10.6f, 1.2f, -1.5f), new Vector3(0.25f, 2.4f, 1.6f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", false, true, 0.55f);

        var exit = new GameObject("ExamExit");
        exit.transform.SetParent(root, false);
        exit.transform.position = new Vector3(-10.2f, 1f, -1.5f);
        var exitBox = exit.AddComponent<BoxCollider>();
        exitBox.isTrigger = true;
        exitBox.size = new Vector3(1.8f, 2.2f, 2.2f);
        exit.AddComponent<ExamExitTrigger>();

        // 左墙在出口处挖空：用缺口标记而不是真布尔，额外放一块「门外」地板引导
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "ExitPad",
            new Vector3(-12.5f, -0.05f, -1.5f), new Vector3(4f, 0.1f, 3f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", true, true, 0.4f);

        // 左墙在 z=-1.5 附近不连续：上面已有整面墙，出口触发在墙内侧；
        // 为方便穿过，在出口处放一个无碰撞通道标记，并关掉该段墙碰撞——
        // 简化：把左墙拆成两段。
        RebuildLeftWallWithGap(root);

        Wire(player, cam, checkpoint.transform);
    }

    static void RebuildLeftWallWithGap(Transform root)
    {
        Transform old = root.Find("WallLeft");
        if (old != null)
            LevelBuildUtil.DestroyObject(old.gameObject);

        // 两段墙，中间 z≈-1.5 留约 2.2m 缺口
        MakeWall(root, "WallLeftA", new Vector3(-11f, 1.8f, 10.5f), new Vector3(0.4f, 3.6f, 22f));
        MakeWall(root, "WallLeftB", new Vector3(-11f, 1.8f, -4.2f), new Vector3(0.4f, 3.6f, 2.2f));
    }

    static void MakeWall(Transform root, string name, Vector3 pos, Vector3 scale)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name, pos, scale, Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
    }

    static void CreateDesks(Transform root)
    {
        // 3 列 × 4 排课桌，作为掩体
        float[] xs = { -5.5f, 0f, 5.5f };
        float[] zs = { 1f, 5f, 9f, 13f };
        int id = 0;
        for (int r = 0; r < zs.Length; r++)
        {
            for (int c = 0; c < xs.Length; c++)
            {
                CreateDesk(root, new Vector3(xs[c], 0f, zs[r]), "Desk_" + id);
                id++;
            }
        }
    }

    static void CreateDesk(Transform root, Vector3 pos, string name)
    {
        var desk = LevelBuildUtil.Make(PrimitiveType.Cube, root, name,
            pos + new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 0.7f, 0.7f), Quaternion.identity,
            EscapePalette.Desk, "Crate", true);
        desk.AddComponent<CoverTag>().label = "课桌";

        LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Chair",
            pos + new Vector3(0f, 0.35f, -0.75f), new Vector3(0.55f, 0.7f, 0.55f), Quaternion.identity,
            EscapePalette.Chair, "BuildingB", true);
    }

    static void CreateCabinet(Transform root, Vector3 pos)
    {
        var cab = LevelBuildUtil.Make(PrimitiveType.Cube, root, "Cabinet",
            pos, new Vector3(1.1f, 2.0f, 0.7f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);
        cab.AddComponent<CoverTag>().label = "柜子";
    }

    static GameObject CreateTeacher(Transform root, Vector3 pos, Quaternion rot, Transform[] route, float patrolSpeed, float chaseSpeed)
    {
        var go = new GameObject("Teacher");
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        go.transform.rotation = rot;

        LevelBuildUtil.Make(PrimitiveType.Capsule, go.transform, "Visual",
            new Vector3(0f, 1f, 0f), new Vector3(0.85f, 1f, 0.85f), Quaternion.identity,
            EscapePalette.Teacher, "CarBody", false);

        var box = go.AddComponent<CapsuleCollider>();
        box.center = new Vector3(0f, 1f, 0f);
        box.height = 2f;
        box.radius = 0.4f;

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = 11f;
        cone.viewAngle = 65f;
        cone.eyeHeight = 1.55f;
        cone.keepCheckingAfterSpot = true;

        var agent = go.AddComponent<TeacherAgent>();
        agent.waypoints = route;
        agent.patrolSpeed = patrolSpeed;
        agent.chaseSpeed = chaseSpeed;
        agent.randomPatrol = true;
        agent.catchDistance = 1.35f;
        return go;
    }

    static Transform MakeWp(Transform root, string name, Vector3 pos)
    {
        var wp = new GameObject(name);
        wp.transform.SetParent(root, false);
        wp.transform.position = pos;
        return wp.transform;
    }

    static void Wire(GameObject player, Camera cam, Transform checkpoint)
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

        if (systems.GetComponent<ExamHallBootstrap>() == null)
            systems.AddComponent<ExamHallBootstrap>();

        StealthGameHUD hud = systems.GetComponent<StealthGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<StealthGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        ExamFlowController flow = systems.GetComponent<ExamFlowController>();
        if (flow == null)
            flow = systems.AddComponent<ExamFlowController>();
        flow.checkpoint = checkpoint;
        flow.player = player.transform;
        flow.hud = hud;
        flow.schoolSceneName = "SchoolEscape";
        flow.autoLoadSchool = true;

        ExamExitTrigger exit = UnityCompat.FindOne<ExamExitTrigger>();
        if (exit != null)
            exit.flow = flow;

        flow.BindTeachers();
        PauseMenu.Ensure();
        follow.Snap();

        // 避免残留开世界 HUD
        MissionHUD legacy = systems.GetComponent<MissionHUD>();
        if (legacy != null)
            LevelBuildUtil.DestroyObject(legacy);
    }
}

using UnityEngine;

/// <summary>
/// 一次生成连通校园大地图：考场→走廊→楼梯/二层→秘密通道→校门→公路/城镇/家庭。
/// 优先尝试实例化 ThirdParty FBX（编辑器）；否则用精细程序化几何兜底。
/// </summary>
public static class CampusWorldBuilder
{
    // 布局原点：考场中心约 (0,0,8)；走廊沿 +Z；校门外公路向 +Z 延伸到 Hub。
    const float FloorY = -0.1f;

    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");
        // 勿销毁 Game：Bootstrap 可能挂在其上，Awake 中自杀会中断生成

        LevelBuildUtil.EnsureSun();
        // 傍晚偏冷，突出审查灯光
        Light sun = UnityCompat.FindOne<Light>();
        if (sun != null)
        {
            sun.color = new Color(0.75f, 0.82f, 0.95f);
            sun.intensity = 0.72f;
            sun.transform.rotation = Quaternion.Euler(38f, -40f, 0f);
        }
        RenderSettings.ambientLight = new Color(0.28f, 0.30f, 0.38f);
        RenderSettings.ambientIntensity = 0.85f;

        Camera cam = LevelBuildUtil.EnsureCamera();
        var root = new GameObject("LevelRoot").transform;

        BuildExamHall(root);
        BuildCorridorAndStairs(root);
        BuildSecretPassages(root);
        BuildSchoolGateAndYard(root);
        BuildRoadAndHub(root);
        BuildHome(root);

        // 尝试摆放第三方模型装饰（编辑器生成时）
        TryPlaceThirdPartyDressing(root);

        Vector3 spawn = new Vector3(0f, 1.02f, -2.2f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);

        var examCp = MakeCheckpoint(root, "ExamCheckpoint", spawn);
        var corridorCp = MakeCheckpoint(root, "CorridorCheckpoint", new Vector3(-14f, 1.02f, 2f));
        var hubCp = MakeCheckpoint(root, "HubCheckpoint", new Vector3(0f, 1.02f, 78f));
        var homeCp = MakeCheckpoint(root, "HomeCheckpoint", new Vector3(-28f, 1.02f, 88f));

        Wire(player, cam, examCp, corridorCp, hubCp, homeCp);
    }

    static Transform MakeCheckpoint(Transform root, string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        return go.transform;
    }

    // -------------------- Exam --------------------
    static void BuildExamHall(Transform root)
    {
        var exam = new GameObject("ExamHall").transform;
        exam.SetParent(root, false);

        // 地面
        LevelBuildUtil.Make(PrimitiveType.Cube, exam, "Floor",
            new Vector3(0f, FloorY, 8f), new Vector3(24f, 0.2f, 28f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);

        // 四面墙（左墙出口留缝）
        MakeWall(exam, "WallBack", new Vector3(0f, 1.9f, -5.8f), new Vector3(24f, 3.8f, 0.4f));
        MakeWall(exam, "WallFront", new Vector3(0f, 1.9f, 21.8f), new Vector3(24f, 3.8f, 0.4f));
        MakeWall(exam, "WallRight", new Vector3(12f, 1.9f, 8f), new Vector3(0.4f, 3.8f, 27.8f));
        // 左墙两段，中间门口
        MakeWall(exam, "WallLeftA", new Vector3(-12f, 1.9f, 11f), new Vector3(0.4f, 3.8f, 21f));
        MakeWall(exam, "WallLeftB", new Vector3(-12f, 1.9f, -4.5f), new Vector3(0.4f, 3.8f, 2.4f));

        // 窗户（右侧）
        for (int i = 0; i < 4; i++)
        {
            float z = 0f + i * 5f;
            LevelBuildUtil.Make(PrimitiveType.Cube, exam, "Window_" + i,
                new Vector3(11.75f, 2.1f, z), new Vector3(0.12f, 1.4f, 2.2f), Quaternion.identity,
                new Color(0.55f, 0.75f, 0.9f, 1f), "Glass", false, true, 0.65f);
        }

        // 黑板 + 讲台
        LevelBuildUtil.Make(PrimitiveType.Cube, exam, "Blackboard",
            new Vector3(0f, 2.3f, 21.45f), new Vector3(9f, 2.4f, 0.12f), Quaternion.identity,
            EscapePalette.Blackboard, "BuildingC", false);
        LevelBuildUtil.Make(PrimitiveType.Cube, exam, "Podium",
            new Vector3(0f, 0.45f, 18.6f), new Vector3(2.6f, 0.9f, 1.3f), Quaternion.identity,
            EscapePalette.Desk, "Crate", true);

        // 课桌 4×4
        float[] xs = { -7f, -2.3f, 2.3f, 7f };
        float[] zs = { 0.5f, 5f, 9.5f, 14f };
        int id = 0;
        for (int r = 0; r < zs.Length; r++)
        {
            for (int c = 0; c < xs.Length; c++)
            {
                CreateDesk(exam, new Vector3(xs[c], 0f, zs[r]), "Desk_" + id);
                id++;
            }
        }

        CreateCabinet(exam, new Vector3(-10f, 1f, 4f));
        CreateCabinet(exam, new Vector3(-10f, 1f, 9f));
        CreateCabinet(exam, new Vector3(10f, 1f, 12f));

        // 出口门视觉
        LevelBuildUtil.Make(PrimitiveType.Cube, exam, "ExitDoor",
            new Vector3(-11.7f, 1.25f, -1.2f), new Vector3(0.2f, 2.5f, 1.8f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", false, true, 0.55f);

        var exit = new GameObject("ExamExit");
        exit.transform.SetParent(exam, false);
        exit.transform.position = new Vector3(-11.2f, 1f, -1.2f);
        var exitBox = exit.AddComponent<BoxCollider>();
        exitBox.isTrigger = true;
        exitBox.size = new Vector3(2.2f, 2.4f, 2.4f);
        exit.AddComponent<ExamExitTrigger>();

        // 老师
        CreateTeacher(exam, "ExamTeacher_Front", new Vector3(0f, 0f, 17.2f), Quaternion.Euler(0f, 180f, 0f),
            new[] { new Vector3(-3f, 0f, 16.5f), new Vector3(3f, 0f, 16.5f), new Vector3(0f, 0f, 18.5f), new Vector3(0f, 0f, 14.5f) },
            1.5f, 4.4f, ScrutinyAura.Role.Teacher, 11f, 65f);
        CreateTeacher(exam, "ExamTeacher_Side", new Vector3(9f, 0f, 10f), Quaternion.Euler(0f, -90f, 0f),
            new[] { new Vector3(9f, 0f, 4f), new Vector3(9f, 0f, 10f), new Vector3(9f, 0f, 16f), new Vector3(7f, 0f, 12f) },
            1.8f, 4.8f, ScrutinyAura.Role.Teacher, 8.5f, 55f);

        // 区域触发
        MakeZone(exam, "Zone_Exam", new Vector3(0f, 1f, 8f), new Vector3(22f, 3f, 26f), CampusFlowController.Zone.Exam);
    }

    // -------------------- Corridor + Stairs + Floor2 --------------------
    static void BuildCorridorAndStairs(Transform root)
    {
        var school = new GameObject("SchoolBuilding").transform;
        school.SetParent(root, false);

        // 走廊从考场门口 (-14,0,-1) 沿 +Z 到约 52
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "CorridorFloor",
            new Vector3(-14f, FloorY, 24f), new Vector3(10f, 0.2f, 56f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);

        // 衔接考场门外地板
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "LinkPad",
            new Vector3(-14f, FloorY, -1.2f), new Vector3(8f, 0.2f, 6f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);

        MakeWall(school, "CorrWallL", new Vector3(-19f, 1.9f, 24f), new Vector3(0.4f, 3.8f, 56f));
        // 右墙分段：楼梯口、储物间口
        MakeWall(school, "CorrWallR_A", new Vector3(-9f, 1.9f, 6f), new Vector3(0.4f, 3.8f, 18f));
        MakeWall(school, "CorrWallR_B", new Vector3(-9f, 1.9f, 30f), new Vector3(0.4f, 3.8f, 18f));
        MakeWall(school, "CorrWallR_C", new Vector3(-9f, 1.9f, 48f), new Vector3(0.4f, 3.8f, 10f));

        // 柜子掩体
        for (int i = 0; i < 6; i++)
            CreateCabinet(school, new Vector3(-17.5f, 1f, 4f + i * 7f));

        // 楼梯（向 +X 进入侧翼，爬到二层）
        BuildStaircase(school, new Vector3(-6f, 0f, 18f));

        // 二层走廊（y=4）
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "Floor2",
            new Vector3(-2f, 3.9f, 28f), new Vector3(14f, 0.25f, 28f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);
        MakeWall(school, "F2_WallL", new Vector3(-9f, 5.8f, 28f), new Vector3(0.35f, 3.6f, 28f));
        MakeWall(school, "F2_WallR", new Vector3(5f, 5.8f, 28f), new Vector3(0.35f, 3.6f, 28f));
        // 二层教室占位 + 侧门
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "F2_RoomFloor",
            new Vector3(-2f, 3.9f, 42f), new Vector3(12f, 0.25f, 10f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "F2_Blackboard",
            new Vector3(-2f, 5.8f, 46.6f), new Vector3(6f, 1.8f, 0.1f), Quaternion.identity,
            EscapePalette.Blackboard, "BuildingC", false);
        CreateDesk(school, new Vector3(-5f, 4f, 40f), "F2_DeskA");
        CreateDesk(school, new Vector3(1f, 4f, 40f), "F2_DeskB");

        // 楼梯护栏
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "StairRail",
            new Vector3(-0.5f, 2.2f, 18f), new Vector3(0.15f, 1.2f, 4f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);

        // 巡逻老师
        CreateTeacher(school, "Patrol_A", new Vector3(-13f, 0f, 12f), Quaternion.identity,
            new[] { new Vector3(-13f, 0f, 6f), new Vector3(-13f, 0f, 16f), new Vector3(-15.5f, 0f, 22f), new Vector3(-12.5f, 0f, 28f) },
            2.0f, 4.8f, ScrutinyAura.Role.Patrol, 9f, 60f);
        CreateTeacher(school, "Patrol_B", new Vector3(-15f, 0f, 36f), Quaternion.identity,
            new[] { new Vector3(-15f, 0f, 30f), new Vector3(-12.5f, 0f, 38f), new Vector3(-15f, 0f, 46f), new Vector3(-13f, 0f, 42f) },
            1.85f, 4.6f, ScrutinyAura.Role.Patrol, 9f, 58f);
        CreateTeacher(school, "Patrol_F2", new Vector3(-2f, 4f, 30f), Quaternion.identity,
            new[] { new Vector3(-5f, 4f, 24f), new Vector3(1f, 4f, 28f), new Vector3(-5f, 4f, 36f), new Vector3(0f, 4f, 40f) },
            1.7f, 4.2f, ScrutinyAura.Role.Patrol, 8f, 55f);

        MakeZone(school, "Zone_School", new Vector3(-14f, 1f, 24f), new Vector3(12f, 4f, 54f), CampusFlowController.Zone.School);
    }

    static void BuildStaircase(Transform parent, Vector3 origin)
    {
        // 8 级台阶到 y≈4
        for (int i = 0; i < 10; i++)
        {
            float y = 0.2f + i * 0.4f;
            float x = origin.x + i * 0.55f;
            LevelBuildUtil.Make(PrimitiveType.Cube, parent, "Step_" + i,
                new Vector3(x, y, origin.z), new Vector3(1.1f, 0.4f, 3.2f), Quaternion.identity,
                new Color(0.55f, 0.52f, 0.48f, 1f), "BuildingA", true);
        }
        // 楼梯平台
        LevelBuildUtil.Make(PrimitiveType.Cube, parent, "StairLanding",
            new Vector3(origin.x + 6.2f, 3.9f, origin.z), new Vector3(3.5f, 0.25f, 4.5f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);
    }

    // -------------------- Secrets --------------------
    static void BuildSecretPassages(Transform root)
    {
        var secrets = new GameObject("Secrets").transform;
        secrets.SetParent(root, false);

        // 通风口：考场后墙内侧 → 走廊中段（蹲走）
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "VentVisual_A",
            new Vector3(-4f, 0.55f, -5.4f), new Vector3(1.6f, 0.9f, 0.2f), Quaternion.identity,
            new Color(0.35f, 0.38f, 0.4f, 1f), "BuildingB", false, true, 0.3f);
        // 通风管道地面（无高大碰撞，可爬行感）
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "VentTunnel",
            new Vector3(-8f, 0.15f, -7.5f), new Vector3(10f, 0.3f, 1.4f), Quaternion.identity,
            new Color(0.3f, 0.32f, 0.34f, 1f), "BuildingC", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "VentTunnel2",
            new Vector3(-14f, 0.15f, 2f), new Vector3(1.4f, 0.3f, 18f), Quaternion.identity,
            new Color(0.3f, 0.32f, 0.34f, 1f), "BuildingC", true);

        var ventEntry = new GameObject("VentEntry");
        ventEntry.transform.SetParent(secrets, false);
        ventEntry.transform.position = new Vector3(-4f, 0.6f, -4.8f);
        var ventDest = new GameObject("VentDest");
        ventDest.transform.SetParent(secrets, false);
        ventDest.transform.position = new Vector3(-14f, 1.0f, 10f);
        var tele = ventEntry.AddComponent<ZoneTeleporter>();
        tele.destination = ventDest.transform;
        tele.prompt = "通风口捷径（建议蹲走按 E）";
        tele.requireCrouch = true;
        tele.interactRadius = 1.8f;
        var ventCol = ventEntry.AddComponent<BoxCollider>();
        ventCol.isTrigger = true;
        ventCol.size = new Vector3(2f, 1.5f, 2f);

        // 储物间侧门：走廊 → 绕过巡逻直通近校门
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "StorageRoom",
            new Vector3(-5.5f, 1.2f, 38f), new Vector3(6f, 2.4f, 5f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);
        // 挖空感：门口
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "StorageDoor",
            new Vector3(-8.7f, 1.2f, 36f), new Vector3(0.2f, 2.2f, 1.4f), Quaternion.identity,
            EscapePalette.Exit, "Glass", false, true, 0.4f);

        var storeEntry = new GameObject("StorageEntry");
        storeEntry.transform.SetParent(secrets, false);
        storeEntry.transform.position = new Vector3(-8.2f, 1f, 36f);
        var storeDest = new GameObject("StorageDest");
        storeDest.transform.SetParent(secrets, false);
        storeDest.transform.position = new Vector3(-14f, 1.02f, 48f);
        var tele2 = storeEntry.AddComponent<ZoneTeleporter>();
        tele2.destination = storeDest.transform;
        tele2.prompt = "储物间侧门捷径";
        tele2.interactRadius = 2f;
        var sc = storeEntry.AddComponent<BoxCollider>();
        sc.isTrigger = true;
        sc.size = new Vector3(2.2f, 2.2f, 2.2f);
    }

    // -------------------- Gate --------------------
    static void BuildSchoolGateAndYard(Transform root)
    {
        var yard = new GameObject("SchoolYard").transform;
        yard.SetParent(root, false);

        LevelBuildUtil.Make(PrimitiveType.Cube, yard, "YardFloor",
            new Vector3(-10f, FloorY, 58f), new Vector3(28f, 0.2f, 16f), Quaternion.identity,
            new Color(0.42f, 0.55f, 0.36f, 1f), "Ground", true);

        LevelBuildUtil.Make(PrimitiveType.Cube, yard, "GateVisual",
            new Vector3(-14f, 1.7f, 52.5f), new Vector3(4f, 3.4f, 0.4f), Quaternion.identity,
            EscapePalette.Gate, "Glass", false, true, 0.5f);
        LevelBuildUtil.Make(PrimitiveType.Cube, yard, "GatePillarL",
            new Vector3(-17f, 1.8f, 52.5f), new Vector3(0.8f, 3.6f, 0.8f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, yard, "GatePillarR",
            new Vector3(-11f, 1.8f, 52.5f), new Vector3(0.8f, 3.6f, 0.8f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        var gate = new GameObject("SchoolGate");
        gate.transform.SetParent(yard, false);
        gate.transform.position = new Vector3(-14f, 1f, 53.2f);
        var box = gate.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(5f, 2.8f, 3f);
        gate.AddComponent<SchoolGateTrigger>();
    }

    // -------------------- Road + Hub --------------------
    static void BuildRoadAndHub(Transform root)
    {
        var hub = new GameObject("HubTownArea").transform;
        hub.SetParent(root, false);

        // 大地面
        LevelBuildUtil.Make(PrimitiveType.Cube, hub, "TownGround",
            new Vector3(0f, FloorY, 95f), new Vector3(90f, 0.2f, 80f), Quaternion.identity,
            EscapePalette.TravelGround, "Sidewalk", true);

        // 公路（程序化条带；编辑器可叠 Kenney road FBX）
        for (int i = 0; i < 12; i++)
        {
            LevelBuildUtil.Make(PrimitiveType.Cube, hub, "Road_" + i,
                new Vector3(-8f, 0.02f, 62f + i * 6f), new Vector3(8f, 0.08f, 6f), Quaternion.identity,
                new Color(0.22f, 0.22f, 0.24f, 1f), "Road", true);
            // 中线
            LevelBuildUtil.Make(PrimitiveType.Cube, hub, "Lane_" + i,
                new Vector3(-8f, 0.08f, 62f + i * 6f), new Vector3(0.25f, 0.04f, 2.5f), Quaternion.identity,
                new Color(0.92f, 0.85f, 0.25f, 1f), "LaneMark", false);
        }

        // 占位楼
        LevelBuildUtil.Make(PrimitiveType.Cube, hub, "BldgA",
            new Vector3(10f, 3.5f, 80f), new Vector3(8f, 7f, 8f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, hub, "BldgB",
            new Vector3(22f, 2.8f, 95f), new Vector3(7f, 5.6f, 7f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true);
        LevelBuildUtil.Make(PrimitiveType.Cube, hub, "BldgC",
            new Vector3(-30f, 3.2f, 100f), new Vector3(9f, 6.4f, 9f), Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);

        // 手机摊
        LevelBuildUtil.Make(PrimitiveType.Cube, hub, "PhoneStall",
            new Vector3(-2f, 0.6f, 78f), new Vector3(2.4f, 1.2f, 1.4f), Quaternion.identity,
            EscapePalette.TicketWindow, "Crate", true);
        var phone = new GameObject("BuyPhone");
        phone.transform.SetParent(hub, false);
        phone.transform.position = new Vector3(-2f, 1f, 76.5f);
        var phoneCol = phone.AddComponent<BoxCollider>();
        phoneCol.isTrigger = true;
        phoneCol.size = new Vector3(3f, 2f, 3f);
        phone.AddComponent<BuyPhoneInteract>();

        CreateEarn(hub, "Earn_Post", new Vector3(8f, 0f, 85f), "送信跑腿", 40);
        CreateEarn(hub, "Earn_Clean", new Vector3(-18f, 0f, 92f), "打扫广场", 35);
        CreateEarn(hub, "Earn_Help", new Vector3(4f, 0f, 100f), "帮摊位搬货", 45);

        // 家门：同场景传送到家庭区
        var homePortal = CreateInWorldPortal(hub, "PortalHome", new Vector3(-22f, 0f, 88f), EscapePalette.Exit,
            "按 E 回家留纸条");
        var homeDest = new GameObject("HomeWarpDest");
        homeDest.transform.SetParent(hub, false);
        homeDest.transform.position = new Vector3(-28f, 1.02f, 112f);
        var ht = homePortal.AddComponent<ZoneTeleporter>();
        ht.destination = homeDest.transform;
        ht.prompt = "按 E 回家留纸条";
        // 去掉 ScenePortal，改用 ZoneTeleporter —— CreateInWorldPortal 返回的是空 GO，需要不挂 ScenePortal

        // 枢纽门：仍可跳到 TransitHub 场景（远端）
        CreateScenePortal(hub, "PortalTransit", new Vector3(12f, 0f, 110f), EscapePalette.Gate,
            "TransitHub", "按 E 进入交通枢纽（需已留条）", false, true, false);

        // Travel 远端提示门（可选）
        CreateScenePortal(hub, "PortalTravelHint", new Vector3(20f, 0f, 118f), EscapePalette.TicketScreen,
            "TravelMissions", "按 E 前往旅行任务区（远端场景）", false, true, false);

        MakeZone(hub, "Zone_Hub", new Vector3(0f, 1f, 95f), new Vector3(80f, 4f, 70f), CampusFlowController.Zone.Hub);
    }

    static GameObject CreateInWorldPortal(Transform root, string name, Vector3 pos, Color color, string prompt)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Visual",
            pos + new Vector3(0f, 1.5f, 0f), new Vector3(2.4f, 3f, 0.4f), Quaternion.identity,
            color, "Glass", false, true, 0.4f);
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos + new Vector3(0f, 1f, 0.8f);
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.5f, 2.5f, 3f);
        return go;
    }

    static void CreateScenePortal(Transform root, string name, Vector3 pos, Color color,
        string scene, string prompt, bool needSchool, bool needNote, bool needPhone)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Visual",
            pos + new Vector3(0f, 1.5f, 0f), new Vector3(2.4f, 3f, 0.4f), Quaternion.identity,
            color, "Glass", false, true, 0.4f);
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

    // -------------------- Home --------------------
    static void BuildHome(Transform root)
    {
        var home = new GameObject("HomeArea").transform;
        home.SetParent(root, false);

        LevelBuildUtil.Make(PrimitiveType.Cube, home, "Floor",
            new Vector3(-28f, FloorY, 118f), new Vector3(16f, 0.2f, 18f), Quaternion.identity,
            EscapePalette.HomeFloor, "Sidewalk", true);
        MakeWall(home, "WallBack", new Vector3(-28f, 1.8f, 126.8f), new Vector3(16f, 3.6f, 0.35f));
        MakeWall(home, "WallFront", new Vector3(-28f, 1.8f, 109.2f), new Vector3(16f, 3.6f, 0.35f));
        // 前门缺口
        MakeWall(home, "WallL", new Vector3(-36f, 1.8f, 118f), new Vector3(0.35f, 3.6f, 18f));
        MakeWall(home, "WallR_A", new Vector3(-20f, 1.8f, 122f), new Vector3(0.35f, 3.6f, 10f));
        MakeWall(home, "WallR_B", new Vector3(-20f, 1.8f, 111f), new Vector3(0.35f, 3.6f, 4f));

        LevelBuildUtil.Make(PrimitiveType.Cube, home, "Table",
            new Vector3(-28f, 0.5f, 122f), new Vector3(2.2f, 0.2f, 1.2f), Quaternion.identity,
            EscapePalette.Desk, "Crate", true);
        var note = new GameObject("LeaveNote");
        note.transform.SetParent(home, false);
        note.transform.position = new Vector3(-28f, 1f, 121f);
        var noteCol = note.AddComponent<BoxCollider>();
        noteCol.isTrigger = true;
        noteCol.size = new Vector3(2.5f, 2f, 2f);
        note.AddComponent<LeaveNoteInteract>();

        LevelBuildUtil.Make(PrimitiveType.Cube, home, "Sofa",
            new Vector3(-32.5f, 0.55f, 116f), new Vector3(2.8f, 1.1f, 1.2f), Quaternion.identity,
            EscapePalette.Cabinet, "BuildingB", true).AddComponent<CoverTag>().label = "沙发";

        // 家长 + 冷紫光
        CreateTeacher(home, "Parent", new Vector3(-25f, 0f, 118f), Quaternion.identity,
            new[] { new Vector3(-25f, 0f, 114f), new Vector3(-25f, 0f, 124f), new Vector3(-31f, 0f, 119f) },
            1.7f, 3.8f, ScrutinyAura.Role.Parent, 8.5f, 60f);

        // 出家门回到城镇
        var back = new GameObject("HomeExit");
        back.transform.SetParent(home, false);
        back.transform.position = new Vector3(-20.5f, 1f, 112f);
        var backDest = new GameObject("HubReturnDest");
        backDest.transform.SetParent(home, false);
        backDest.transform.position = new Vector3(-20f, 1.02f, 90f);
        var bt = back.AddComponent<ZoneTeleporter>();
        bt.destination = backDest.transform;
        bt.prompt = "按 E 返回城镇";
        var bc = back.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(2.5f, 2.5f, 2.5f);
        LevelBuildUtil.Make(PrimitiveType.Cube, home, "HomeExitDoor",
            new Vector3(-20.2f, 1.4f, 112f), new Vector3(0.25f, 2.6f, 1.6f), Quaternion.identity,
            EscapePalette.Exit, "Glass", false, true, 0.45f);

        MakeZone(home, "Zone_Home", new Vector3(-28f, 1f, 118f), new Vector3(14f, 3f, 16f), CampusFlowController.Zone.Home);
    }

    static void TryPlaceThirdPartyDressing(Transform root)
    {
        // 校门外摆一两栋 suburban 建筑 + 公路块（仅编辑器非 Play）
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.SchoolBuilding, root,
            new Vector3(6f, 0f, 55f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.0f);
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.BuildingA, root,
            new Vector3(18f, 0f, 88f), Quaternion.identity, Vector3.one * 1.2f);
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.BuildingB, root,
            new Vector3(28f, 0f, 100f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 1.1f);
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.TreeSmall, root,
            new Vector3(-2f, 0f, 70f), Quaternion.identity, Vector3.one);
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.RoadStraight, root,
            new Vector3(-8f, 0f, 68f), Quaternion.identity, Vector3.one * 2f);
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.FurnitureBookcase, root,
            new Vector3(10f, 0f, 12f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
        // 整间教室 FBX 作装饰旁楼（避免与程序化考场重叠）
        ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.Classroom, root,
            new Vector3(18f, 0f, 8f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
    }

    // -------------------- Shared helpers --------------------
    static void MakeWall(Transform root, string name, Vector3 pos, Vector3 scale)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name, pos, scale, Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
    }

    static void CreateDesk(Transform root, Vector3 pos, string name)
    {
        GameObject model = ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.FurnitureDesk, root,
            pos + new Vector3(0f, 0f, 0f), Quaternion.identity, Vector3.one);
        if (model == null)
        {
            var desk = LevelBuildUtil.Make(PrimitiveType.Cube, root, name,
                pos + new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 0.7f, 0.7f), Quaternion.identity,
                EscapePalette.Desk, "Crate", true);
            desk.AddComponent<CoverTag>().label = "课桌";
            LevelBuildUtil.Make(PrimitiveType.Cube, root, name + "_Chair",
                pos + new Vector3(0f, 0.35f, -0.75f), new Vector3(0.55f, 0.7f, 0.55f), Quaternion.identity,
                EscapePalette.Chair, "BuildingB", true);
        }
        else
        {
            model.name = name;
            model.AddComponent<CoverTag>().label = "课桌";
            // 简单碰撞
            if (model.GetComponentInChildren<Collider>() == null)
            {
                var box = model.AddComponent<BoxCollider>();
                box.size = new Vector3(1.2f, 0.8f, 0.7f);
                box.center = new Vector3(0f, 0.4f, 0f);
            }
            ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.FurnitureChairDesk, root,
                pos + new Vector3(0f, 0f, -0.85f), Quaternion.identity, Vector3.one);
        }
    }

    static void CreateCabinet(Transform root, Vector3 pos)
    {
        GameObject model = ThirdPartyModelUtil.TrySpawn(ThirdPartyModelUtil.FurnitureCabinet, root,
            pos + new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one);
        if (model == null)
        {
            var cab = LevelBuildUtil.Make(PrimitiveType.Cube, root, "Cabinet",
                pos, new Vector3(1.1f, 2.0f, 0.7f), Quaternion.identity,
                EscapePalette.Cabinet, "BuildingB", true);
            cab.AddComponent<CoverTag>().label = "柜子";
        }
        else
        {
            model.AddComponent<CoverTag>().label = "柜子";
            if (model.GetComponentInChildren<Collider>() == null)
                model.AddComponent<BoxCollider>();
        }
    }

    static GameObject CreateTeacher(Transform root, string name, Vector3 pos, Quaternion rot,
        Vector3[] route, float patrolSpeed, float chaseSpeed, ScrutinyAura.Role role,
        float viewDist, float viewAngle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        go.transform.rotation = rot;

        Color body = role == ScrutinyAura.Role.Parent ? EscapePalette.Parent
            : (role == ScrutinyAura.Role.Patrol ? EscapePalette.Patrol : EscapePalette.Teacher);
        LevelBuildUtil.Make(PrimitiveType.Capsule, go.transform, "Visual",
            new Vector3(0f, 1f, 0f), new Vector3(0.85f, 1f, 0.85f), Quaternion.identity,
            body, "CarBody", false);

        var col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1f, 0f);
        col.height = 2f;
        col.radius = 0.4f;

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = viewDist;
        cone.viewAngle = viewAngle;
        cone.eyeHeight = 1.55f;
        cone.keepCheckingAfterSpot = role != ScrutinyAura.Role.Parent;
        cone.gizmoColor = role == ScrutinyAura.Role.Parent
            ? new Color(0.6f, 0.3f, 1f, 0.35f)
            : new Color(1f, 0.25f, 0.1f, 0.35f);

        var wps = new Transform[route.Length];
        for (int i = 0; i < route.Length; i++)
        {
            var wp = new GameObject(name + "_WP" + i);
            wp.transform.SetParent(root, false);
            wp.transform.position = route[i];
            wps[i] = wp.transform;
        }

        var agent = go.AddComponent<TeacherAgent>();
        agent.waypoints = wps;
        agent.patrolSpeed = patrolSpeed;
        agent.chaseSpeed = chaseSpeed;
        agent.randomPatrol = true;
        agent.catchDistance = 1.35f;

        var aura = go.AddComponent<ScrutinyAura>();
        aura.SetRole(role);
        return go;
    }

    static void MakeZone(Transform root, string name, Vector3 pos, Vector3 size, CampusFlowController.Zone zone)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        var z = go.AddComponent<CampusZoneTrigger>();
        z.zone = zone;
    }

    static void Wire(GameObject player, Camera cam,
        Transform examCp, Transform corridorCp, Transform hubCp, Transform homeCp)
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
        if (systems.GetComponent<CampusWorldBootstrap>() == null)
            systems.AddComponent<CampusWorldBootstrap>();

        StealthGameHUD hud = systems.GetComponent<StealthGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<StealthGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        HubTownHUD hubHud = systems.GetComponent<HubTownHUD>();
        if (hubHud == null)
            hubHud = systems.AddComponent<HubTownHUD>();
        if (hubHud.uiFont == null)
            hubHud.uiFont = Resources.Load<Font>("UIFont");

        WorldMapUI map = systems.GetComponent<WorldMapUI>();
        if (map == null)
            map = systems.AddComponent<WorldMapUI>();
        if (map.uiFont == null)
            map.uiFont = Resources.Load<Font>("UIFont");

        CampusFlowController flow = systems.GetComponent<CampusFlowController>();
        if (flow == null)
            flow = systems.AddComponent<CampusFlowController>();
        flow.player = player.transform;
        flow.examCheckpoint = examCp;
        flow.corridorCheckpoint = corridorCp;
        flow.hubCheckpoint = hubCp;
        flow.homeCheckpoint = homeCp;
        flow.activeCheckpoint = examCp;
        flow.hud = hud;
        flow.hubHud = hubHud;
        flow.mapUi = map;

        // 出口接到 CampusFlow（同场景）
        ExamExitTrigger examExit = UnityCompat.FindOne<ExamExitTrigger>();
        if (examExit != null)
            examExit.campusFlow = flow;

        SchoolGateTrigger gate = UnityCompat.FindOne<SchoolGateTrigger>();
        if (gate != null)
            gate.campusFlow = flow;

        // Hub 赚钱 / 场景门
        HubTownFlowController hubFlow = systems.GetComponent<HubTownFlowController>();
        if (hubFlow == null)
            hubFlow = systems.AddComponent<HubTownFlowController>();
        hubFlow.hud = hubHud;
        hubFlow.mapUi = map;
        hubFlow.player = player.transform;

        EarnMoneyPoint[] earns = UnityCompat.FindAll<EarnMoneyPoint>();
        for (int i = 0; i < earns.Length; i++)
        {
            if (earns[i] != null)
                earns[i].flow = hubFlow;
        }
        ScenePortalInteract[] portals = UnityCompat.FindAll<ScenePortalInteract>();
        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null)
                portals[i].flow = hubFlow;
        }

        // 家庭留条：若无独立 HomeFlow，挂一个轻量适配
        HomeFlowController homeFlow = systems.GetComponent<HomeFlowController>();
        if (homeFlow == null)
            homeFlow = systems.AddComponent<HomeFlowController>();
        homeFlow.checkpoint = homeCp;
        homeFlow.player = player.transform;
        homeFlow.hud = hud;
        homeFlow.autoReturnHub = false; // 同场景，不跳转
        LeaveNoteInteract note = UnityCompat.FindOne<LeaveNoteInteract>();
        if (note != null)
            note.flow = homeFlow;

        CampusZoneTrigger[] zones = UnityCompat.FindAll<CampusZoneTrigger>();
        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i] != null)
                zones[i].flow = flow;
        }

        flow.BindTeachers();
        homeFlow.BindDetectors();
        PauseMenu.Ensure();
        follow.Snap();
    }
}

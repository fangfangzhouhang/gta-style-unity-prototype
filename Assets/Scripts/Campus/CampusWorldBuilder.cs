using UnityEngine;

/// <summary>
/// 一次生成连通校园大地图：考场→走廊→楼梯/二层→秘密通道→校门→公路/城镇/家庭。
/// 优先尝试实例化 ThirdParty FBX（编辑器）；否则用精细程序化几何兜底。
/// </summary>
public static class CampusWorldBuilder
{
    // 布局原点：考场中心约 (0,0,8)；走廊沿 +Z；校门外公路向 +Z 延伸到 Hub。
    const float FloorY = -0.1f;
    public const float ExamFloorHeight = 4f;

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
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.65f);
        RenderSettings.ambientIntensity = 0.85f;

        Camera cam = LevelBuildUtil.EnsureCamera();
        var root = new GameObject("LevelRoot").transform;

        BuildExamHall(root);
        BuildCorridorAndStairs(root);
        BuildSecretPassages(root);
        BuildSchoolGateAndYard(root);
        BuildRoadAndHub(root);
        BuildHome(root);
        CampusExpansion.Build(root);
        root.gameObject.AddComponent<CampusNavigation>();

        // 教学楼和街区统一按米制地块生成。
        CampusSitePlan.Build(root);

        Vector3 spawn = new Vector3(-7.5f, ExamFloorHeight + 0.92f, -2.2f);
        new GameObject("CampusLayout_v11").transform.SetParent(root, false);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);
        player.GetComponent<ThirdPersonController>().ConfigureHumanScale();
        var avoidance=player.AddComponent<UnityEngine.AI.NavMeshObstacle>();
        avoidance.shape=UnityEngine.AI.NavMeshObstacleShape.Capsule;
        avoidance.height=1.8f; avoidance.radius=0.32f; avoidance.carving=false;
        player.AddComponent<CampusSafety>();
        CampusMotorcycle.Create(root, player.GetComponent<ThirdPersonController>(), cam.GetComponent<ThirdPersonCamera>());

        var examCp = MakeCheckpoint(root, "ExamCheckpoint", spawn);
        var corridorCp = MakeCheckpoint(root, "CorridorCheckpoint", new Vector3(-14.4f, ExamFloorHeight + 0.92f, -1.2f));
        var hubCp = MakeCheckpoint(root, "HubCheckpoint", new Vector3(0f, 1.02f, 78f));
        var homeCp = MakeCheckpoint(root, "HomeCheckpoint", new Vector3(-28f, 0.92f, 115f));

        Wire(player, cam, examCp, corridorCp, hubCp, homeCp);
        root.gameObject.AddComponent<CampusEscapeDirector>().BuildObjects();
        CampusExamChapter.Build(root);
        CampusCorridorChapter.Build(root);
        CampusLibraryChapter.Build(root);
        CampusDepartureChapter.Build(root);
        root.gameObject.AddComponent<CampusNarrative>();
        root.gameObject.AddComponent<CampusJournal>();
        root.gameObject.AddComponent<CampusSaveRestore>();
        // Rear investigation area stays outside the side proctor's sight range.
        GameObject.Find("ExamTeacher_Side").GetComponent<VisionConeDetector>().viewDistance=4.5f;
        var groundCp = MakeCheckpoint(root,"GroundSchoolCheckpoint",new Vector3(-14.4f,0.92f,32.5f));
        var checkpointTrigger = new GameObject("GroundCheckpointTrigger");
        checkpointTrigger.transform.SetParent(root,false);
        checkpointTrigger.transform.localPosition = new Vector3(-14.4f,1.1f,33.5f);
        var checkpointBox=checkpointTrigger.AddComponent<BoxCollider>(); checkpointBox.isTrigger=true; checkpointBox.size=new Vector3(4f,2.2f,2.4f);
        checkpointTrigger.AddComponent<CampusCheckpoint>().checkpoint=groundCp;
        var eastCp=MakeCheckpoint(root,"EastGroundCheckpoint",new Vector3(48,.92f,12.1f));
        var eastTrigger=new GameObject("EastGroundCheckpointTrigger");eastTrigger.transform.SetParent(root,false);eastTrigger.transform.position=new Vector3(48,1,12.1f);
        var eastBox=eastTrigger.AddComponent<BoxCollider>();eastBox.isTrigger=true;eastBox.size=new Vector3(2.8f,2,1.6f);
        eastTrigger.AddComponent<CampusCheckpoint>().checkpoint=eastCp;
        if (!Application.isPlaying) LevelBuildUtil.BakeCampusMaterials();
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

        // 米制教室：9m × 12m，净高 3.4m。西侧门沿用原走廊连接位置。
        DeskPart(exam, "Floor", new Vector3(-7.5f, -0.1f, 2f), new Vector3(9f, 0.2f, 12f), EscapePalette.Floor);
        MakeWall(exam, "WallBack", new Vector3(-7.5f, 1.7f, -4f), new Vector3(9.4f, 3.4f, 0.2f));
        MakeWall(exam, "WallFront", new Vector3(-7.5f, 1.7f, 8f), new Vector3(9.4f, 3.4f, 0.2f));
        // 窗洞真实开口，上下墙带闭合，不用整面实墙盖住窗。
        MakeWall(exam, "WindowSillWall", new Vector3(-3f, 0.5f, 2f), new Vector3(0.2f, 1f, 12f));
        MakeWall(exam, "WindowLintelWall", new Vector3(-3f, 3.05f, 2f), new Vector3(0.2f, 0.7f, 12f));
        foreach (float z in new[] { -3.75f, -0.9f, 2f, 4.9f, 7.75f })
            MakeWall(exam, "WindowPier", new Vector3(-3f, 1.85f, z), new Vector3(0.2f, 1.7f, 0.5f));
        MakeWall(exam, "WallLeftA", new Vector3(-12f, 1.7f, 3.95f), new Vector3(0.2f, 3.4f, 8.1f));
        MakeWall(exam, "WallLeftB", new Vector3(-12f, 1.7f, -3.15f), new Vector3(0.2f, 3.4f, 1.7f));
        MakeWall(exam, "DoorLintel", new Vector3(-12f, 2.85f, -1.2f), new Vector3(0.2f, 1.1f, 2.2f));
        DeskPart(exam, "Blackboard", new Vector3(-7.5f, 2.05f, 7.85f), new Vector3(4f, 1.25f, 0.08f), EscapePalette.Blackboard, false);
        DeskPart(exam, "Podium", new Vector3(-7.5f, 0.51f, 7f), new Vector3(1.2f, 1.02f, 0.6f), EscapePalette.Desk);
        float[] xs = { -10.2f, -8.6f, -7f, -5.4f };
        float[] zs = { 0.35f, 1.75f, 3.15f, 4.55f, 5.95f };
        int id = 0;
        foreach (float z in zs) foreach (float x in xs)
            CreateDesk(exam, new Vector3(x, 0f, z), "Desk_" + id++);
        CreateCabinet(exam, new Vector3(-11.2f, 1f, 2f));
        CreateCabinet(exam, new Vector3(-11.2f, 1f, 5f));
        DeskPart(exam, "ExitDoor", new Vector3(-11.92f, 1.1f, -1.2f), new Vector3(0.03f, 2.2f, 1.1f), EscapePalette.Exit, false);
        var exit = new GameObject("ExamExit");
        exit.transform.SetParent(exam, false);
        exit.transform.localPosition = new Vector3(-12.05f, 1f, -1.2f);
        var exitBox = exit.AddComponent<BoxCollider>();
        exitBox.isTrigger = true;
        exitBox.size = new Vector3(1.5f, 2.2f, 1.7f);
        exit.AddComponent<ExamExitTrigger>();
        CreateTeacher(exam, "ExamTeacher_Front", new Vector3(-7.5f, 0f, 6.5f), Quaternion.Euler(0f, 180f, 0f),
            new[] { new Vector3(-10f, 0f, 6.6f), new Vector3(-5f, 0f, 6.6f) },
            0.8f, 2.8f, ScrutinyAura.Role.Teacher, 7f, 65f);
        CreateTeacher(exam, "ExamTeacher_Side", new Vector3(-4.1f, 0f, 4f), Quaternion.Euler(0f, -90f, 0f),
            new[] { new Vector3(-4.1f, 0f, 1f), new Vector3(-4.1f, 0f, 6f) },
            0.9f, 3f, ScrutinyAura.Role.Teacher, 6f, 55f);
        MakeZone(exam, "Zone_Exam", new Vector3(-7.5f, 1.5f, 2f), new Vector3(8.8f, 3f, 11.8f), CampusFlowController.Zone.Exam);
        DressClassroom(exam);
        AddCeiling(exam, "ExamCeiling", new Vector3(-7.5f, 3.5f, 2f), new Vector3(9.4f, 0.2f, 12.4f));
        exam.localPosition = Vector3.up * ExamFloorHeight;
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

        MakeWall(school, "CorrWallL", new Vector3(-19f, 1.9f, 24f), new Vector3(0.4f, 3.8f, 56f));
        // 右墙分段：楼梯口、储物间口
        MakeWall(school, "CorrWallR_A", new Vector3(-9f, 1.9f, 6f), new Vector3(0.4f, 3.8f, 18f));
        MakeWall(school, "CorrWallR_B", new Vector3(-9f, 1.9f, 30f), new Vector3(0.4f, 3.8f, 18f));
        MakeWall(school, "CorrWallR_C", new Vector3(-9f, 1.9f, 48f), new Vector3(0.4f, 3.8f, 10f));

        // 柜子掩体
        for (int i = 0; i < 6; i++)
            CreateCabinet(school, new Vector3(-17.5f, 1f, 4f + i * 7f));

        // 二楼考场门外的连续走廊，末端接下行楼梯。
        LevelBuildUtil.Make(PrimitiveType.Cube, school, "UpperCorridor",
            new Vector3(-14.4f, 3.9f, 9f), new Vector3(5.2f, 0.2f, 26f), Quaternion.identity,
            EscapePalette.Corridor, "Sidewalk", true);
        MakeWall(school, "UpperBack", new Vector3(-14.4f, 5.7f, -4f), new Vector3(5.2f, 3.4f, 0.2f));
        MakeWall(school, "UpperLeftRail", new Vector3(-17f, 5.7f, 9f), new Vector3(0.16f, 3.4f, 26f));
        MakeWall(school, "UpperRightRail", new Vector3(-11.8f, 5.7f, 11.3f), new Vector3(0.16f, 3.4f, 21.4f));
        BuildStaircase(school, new Vector3(-14.4f, 0f, 22f));
        AddCeiling(school, "UpperCorridorCeiling", new Vector3(-14.4f, 7.5f, 9f), new Vector3(5.4f, 0.2f, 26.2f));
        AddCeiling(school, "GroundCorridorCeiling", new Vector3(-14f, 3.9f, 42f), new Vector3(10f, 0.2f, 20f));
        AddCeiling(school, "GroundSideCeilingL", new Vector3(-18.05f, 3.9f, 9f), new Vector3(2.1f, 0.2f, 26f));
        AddCeiling(school, "GroundSideCeilingR", new Vector3(-10.35f, 3.9f, 9f), new Vector3(2.9f, 0.2f, 26f));
        DeskPart(school, "StairCeiling", new Vector3(-14.4f, 5.5f, 27f), new Vector3(5.4f, 0.2f, 10.95f), new Color(0.9f, 0.9f, 0.86f));
        school.Find("StairCeiling").localRotation = Quaternion.Euler(21.8f, 0f, 0f);
        MakeWall(school, "StairEnclosureL", new Vector3(-17f, 3.7f, 27f), new Vector3(0.16f, 7.4f, 10f));
        MakeWall(school, "StairEnclosureR", new Vector3(-11.8f, 3.7f, 27f), new Vector3(0.16f, 7.4f, 10f));
        for (int i = 0; i < 4; i++)
            AddRoomLight(school, new Vector3(-14.4f, 7.25f, -1f + i * 6f), 3.6f);
        AddRoomLight(school, new Vector3(-14.4f, 5.5f, 27f), 5f);
        AddRoomLight(school, new Vector3(-14f, 3.6f, 38f), 5f);
        AddRoomLight(school, new Vector3(-14f, 3.6f, 48f), 5f);

        MakeZone(school, "Zone_UpperSchool", new Vector3(-14.4f, 5.3f, 9f),
            new Vector3(5f, 2.4f, 26f), CampusFlowController.Zone.School);
        MakeSign(school, "Floor2Sign", "2F  考场 · 安全出口 →", new Vector3(-16.85f, 6f, 14f), Quaternion.Euler(0f, 90f, 0f));
        MakeSign(school, "StairSign", "安全出口 ↓ 1F", new Vector3(-14.4f, 6.8f, 21.5f), Quaternion.identity);
        MakeSign(school, "Floor1Sign", "1F  校门 →", new Vector3(-18.75f, 2.5f, 37f), Quaternion.Euler(0f, 90f, 0f));

        // 巡逻老师
        CreateTeacher(school, "Patrol_A", new Vector3(-13f, 0f, 12f), Quaternion.identity,
            new[] { new Vector3(-13f, 0f, 6f), new Vector3(-13f, 0f, 16f), new Vector3(-15.5f, 0f, 22f), new Vector3(-12.5f, 0f, 28f) },
            2.0f, 4.8f, ScrutinyAura.Role.Patrol, 9f, 60f);
        CreateTeacher(school, "Patrol_B", new Vector3(-15f, 0f, 36f), Quaternion.identity,
            new[] { new Vector3(-15f, 0f, 30f), new Vector3(-12.5f, 0f, 38f), new Vector3(-15f, 0f, 46f), new Vector3(-13f, 0f, 42f) },
            1.85f, 4.6f, ScrutinyAura.Role.Patrol, 9f, 58f);
        CreateTeacher(school, "Patrol_F2", new Vector3(-14.4f, 4f, 18f), Quaternion.identity,
            new[] { new Vector3(-14.4f, 4f, 16f), new Vector3(-14.4f, 4f, 20f) },
            1.5f, 3.8f, ScrutinyAura.Role.Patrol, 7f, 55f);

        MakeZone(school, "Zone_School", new Vector3(-14f, 1f, 24f), new Vector3(12f, 4f, 54f), CampusFlowController.Zone.School);
    }

    static void BuildStaircase(Transform parent, Vector3 origin)
    {
        // 每级 0.20m，低于 CharacterController.stepOffset=0.35m。
        for (int i = 0; i < 20; i++)
        {
            float top = ExamFloorHeight - (i + 1) * 0.2f;
            float height = Mathf.Max(0.08f, top);
            LevelBuildUtil.Make(PrimitiveType.Cube, parent, "EscapeStep_" + i,
                new Vector3(origin.x, top - height * 0.5f, origin.z + 0.25f + i * 0.5f),
                new Vector3(3.2f, height, 0.5f), Quaternion.identity,
                new Color(0.62f, 0.65f, 0.64f), "Sidewalk", true);
            // 窄色条标识踏步前缘。
            LevelBuildUtil.Make(PrimitiveType.Cube, parent, "StepEdge_" + i,
                new Vector3(origin.x, top + 0.006f, origin.z + 0.47f + i * 0.5f),
                new Vector3(3.2f, 0.012f, 0.04f), Quaternion.identity,
                new Color(0.8f, 0.69f, 0.3f), "LaneMark", false);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            LevelBuildUtil.Make(PrimitiveType.Cube, parent, "StairHandrail",
                new Vector3(origin.x + side * 1.66f, 3f, origin.z + 5f),
                new Vector3(0.1f, 0.1f, 10.77f), Quaternion.Euler(21.8f, 0f, 0f),
                EscapePalette.Cabinet, "BuildingB", true);
            for (int i = 0; i <= 10; i++)
                MakeWall(parent, "RailPost", new Vector3(origin.x + side * 1.66f,
                    4.5f - i * 0.4f, origin.z + i), new Vector3(0.08f, 1f, 0.08f));
        }
    }

    // -------------------- Secrets --------------------
    static void BuildSecretPassages(Transform root)
    {
        var secrets = new GameObject("Secrets").transform;
        secrets.SetParent(root, false);

        // 通风口：考场后墙内侧 → 走廊中段（蹲走）
        LevelBuildUtil.Make(PrimitiveType.Cube, secrets, "VentVisual_A",
            new Vector3(-10.7f, 4.55f, -3.85f), new Vector3(1.6f, 0.9f, 0.2f), Quaternion.identity,
            new Color(0.35f, 0.38f, 0.4f, 1f), "BuildingB", false, true, 0.3f);
        var ventEntry = new GameObject("VentEntry");
        ventEntry.transform.SetParent(secrets, false);
        ventEntry.transform.position = new Vector3(-10.7f, 4.6f, -3.45f);
        var ventDest = new GameObject("VentDest");
        ventDest.transform.SetParent(secrets, false);
        ventDest.transform.position = new Vector3(-14.4f, 4.92f, 10f);
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

        // 公路（程序化条带；编辑器可叠 Kenney road FBX）
        for (int i = 0; i < 26; i++)
        {
            LevelBuildUtil.Make(PrimitiveType.Cube, hub, "Road_" + i,
                new Vector3(-8f, 0.02f, 62f + i * 6f), new Vector3(8f, 0.08f, 6f), Quaternion.identity,
                new Color(0.22f, 0.22f, 0.24f, 1f), "Road", true);
            // 中线
            LevelBuildUtil.Make(PrimitiveType.Cube, hub, "Lane_" + i,
                new Vector3(-8f, 0.08f, 62f + i * 6f), new Vector3(0.25f, 0.04f, 2.5f), Quaternion.identity,
                new Color(0.92f, 0.85f, 0.25f, 1f), "LaneMark", false);
        }

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
        homeDest.transform.position = new Vector3(-28f, 0.92f, 115f);
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

        MakeZone(hub, "Zone_Hub", new Vector3(0f, 1f, 125f), new Vector3(176f, 4f, 176f), CampusFlowController.Zone.Hub);
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
        DeskPart(home, "Floor", new Vector3(-28f, -0.1f, 118f), new Vector3(8.8f, 0.2f, 9f), EscapePalette.HomeFloor);
        MakeWall(home, "WallBack", new Vector3(-28f, 1.6f, 122.5f), new Vector3(9f, 3.2f, 0.2f));
        MakeWall(home, "WallFront", new Vector3(-28f, 1.6f, 113.5f), new Vector3(9f, 3.2f, 0.2f));
        MakeWall(home, "WallL", new Vector3(-32.4f, 1.6f, 118f), new Vector3(0.2f, 3.2f, 9f));
        MakeWall(home, "WallR_A", new Vector3(-23.6f, 1.6f, 119.2f), new Vector3(0.2f, 3.2f, 6.6f));
        MakeWall(home, "WallR_B", new Vector3(-23.6f, 1.6f, 114f), new Vector3(0.2f, 3.2f, 1f));
        MakeWall(home, "DoorLintel", new Vector3(-23.6f, 2.7f, 115.2f), new Vector3(0.2f, 1f, 1.4f));
        DeskPart(home, "Table", new Vector3(-28f, 0.725f, 121f), new Vector3(1.4f, 0.05f, 0.75f), EscapePalette.Desk);
        foreach (float x in new[] { -28.6f, -27.4f }) foreach (float z in new[] { 120.7f, 121.3f })
            DeskPart(home, "TableLeg", new Vector3(x, 0.35f, z), new Vector3(0.06f, 0.7f, 0.06f), EscapePalette.Cabinet);
        var note = new GameObject("LeaveNote");
        note.transform.SetParent(home, false);
        note.transform.localPosition = new Vector3(-28f, 1f, 120.3f);
        var noteCol = note.AddComponent<BoxCollider>();
        noteCol.isTrigger = true;
        noteCol.size = new Vector3(1.5f, 2f, 1.5f);
        note.AddComponent<LeaveNoteInteract>();
        DeskPart(home, "Sofa", new Vector3(-30.7f, 0.4f, 117f), new Vector3(2.2f, 0.8f, 0.9f), EscapePalette.Cabinet);
        DeskPart(home, "SofaBack", new Vector3(-30.7f, 0.85f, 117.4f), new Vector3(2.2f, 0.6f, 0.15f), EscapePalette.Cabinet);
        CreateTeacher(home, "Parent", new Vector3(-25.5f, 0f, 119f), Quaternion.identity,
            new[] { new Vector3(-25.5f, 0f, 116f), new Vector3(-25.5f, 0f, 121f), new Vector3(-29f, 0f, 118f) },
            0.8f, 2.6f, ScrutinyAura.Role.Parent, 6f, 60f);
        var back = new GameObject("HomeExit");
        back.transform.SetParent(home, false);
        back.transform.localPosition = new Vector3(-24f, 1f, 115.2f);
        var backDest = new GameObject("HubReturnDest");
        backDest.transform.SetParent(home, false);
        backDest.transform.localPosition = new Vector3(-20f, 0.92f, 90f);
        var tele = back.AddComponent<ZoneTeleporter>();
        tele.destination = backDest.transform;
        tele.prompt = "按 E 返回城镇";
        DeskPart(home, "HomeExitDoor", new Vector3(-23.72f, 1.1f, 115.2f), new Vector3(0.04f, 2.2f, 1.1f), EscapePalette.Exit, false);
        AddCeiling(home, "HomeCeiling", new Vector3(-28f, 3.3f, 118f), new Vector3(9f, 0.2f, 9.2f));
        foreach (float z in new[] { 115f, 118f, 121f }) AddRoomLight(home, new Vector3(-28f, 3.05f, z), 5f);
        MakeZone(home, "Zone_Home", new Vector3(-28f, 1.5f, 118f), new Vector3(8.4f, 3f, 8.6f), CampusFlowController.Zone.Home);
    }

    // -------------------- Shared helpers --------------------
    static void MakeWall(Transform root, string name, Vector3 pos, Vector3 scale)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name, pos, scale, Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
    }

    static void CreateDesk(Transform root, Vector3 pos, string name)
    {
        // 固定米制尺寸，编辑器与运行时均使用同一模型，避免 FBX 单位不一致。
        var desk = new GameObject(name).transform;
        desk.SetParent(root, false);
        desk.localPosition = pos;
        DeskPart(desk, "Top", new Vector3(0f, 0.735f, 0f), new Vector3(0.70f, 0.05f, 0.50f), EscapePalette.Desk);
        DeskPart(desk, "Shelf", new Vector3(0f, 0.57f, 0f), new Vector3(0.60f, 0.04f, 0.40f), EscapePalette.Desk);
        foreach (float x in new[] { -0.28f, 0.28f })
            foreach (float z in new[] { -0.19f, 0.19f })
                DeskPart(desk, "Leg", new Vector3(x, 0.35f, z), new Vector3(0.035f, 0.70f, 0.035f), EscapePalette.Cabinet);
        DeskPart(desk, "ChairSeat", new Vector3(0f, 0.435f, -0.72f), new Vector3(0.40f, 0.04f, 0.40f), EscapePalette.Chair);
        DeskPart(desk, "ChairBack", new Vector3(0f, 0.69f, -0.91f), new Vector3(0.40f, 0.35f, 0.035f), EscapePalette.Chair);
        foreach (float x in new[] { -0.16f, 0.16f })
            foreach (float z in new[] { -0.88f, -0.56f })
                DeskPart(desk, "ChairLeg", new Vector3(x, 0.21f, z), new Vector3(0.03f, 0.42f, 0.03f), EscapePalette.Cabinet);
        DeskPart(desk, "ExamPaper", new Vector3(0f, 0.764f, 0.02f), new Vector3(0.21f, 0.005f, 0.29f), Color.white, false);
        desk.gameObject.AddComponent<CoverTag>().label = "课桌";
    }

    static void DeskPart(Transform root, string name, Vector3 pos, Vector3 size, Color color, bool solid = true)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, root, name, pos, size, Quaternion.identity,
            color, "Campus_" + name, solid);
    }

    static void MakeSign(Transform root, string name, string label, Vector3 pos, Quaternion rotation)
    {
        var sign = new GameObject(name);
        sign.transform.SetParent(root, false);
        sign.transform.localPosition = pos;
        sign.transform.localRotation = rotation;
        var text = sign.AddComponent<TextMesh>();
        text.text = label;
        text.font = Resources.Load<Font>("UIFont");
        text.fontSize = 48;
        text.characterSize = 0.035f;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = new Color(0.18f, 0.42f, 0.30f);
        if (text.font != null) sign.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        sign.AddComponent<CampusWorldSign>();
    }

    static void AddCeiling(Transform root, string name, Vector3 position, Vector3 size)
    {
        DeskPart(root, name, position, size, new Color(0.9f, 0.9f, 0.86f));
    }

    static void AddRoomLight(Transform root, Vector3 position, float range)
    {
        DeskPart(root,"FluorescentHousing",position,new Vector3(1.2f,0.055f,0.25f),new Color(0.64f,0.66f,0.65f),false);
        foreach (float z in new[] {-0.065f,0.065f})
            LevelBuildUtil.Make(PrimitiveType.Cylinder,root,"FluorescentTube",position+new Vector3(0,-0.04f,z),
                new Vector3(0.025f,0.54f,0.025f),Quaternion.Euler(0,0,90),new Color(0.9f,0.96f,1f),"TubeEmission",false,true,0.3f);
        var fixture = new GameObject("InteriorLight");
        fixture.transform.SetParent(root, false);
        fixture.transform.localPosition = position - Vector3.up * 0.12f;
        var light = fixture.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.9f,0.95f,1f);
        light.intensity = 0.8f;
        light.range = range;
        light.shadows = LightShadows.None;
        if (position.x < -9f && position.z > 2f && position.z < 4f)
        { light.shadows=LightShadows.Soft; light.shadowBias=0.025f; light.shadowNormalBias=0.12f; }
    }

    static void DressClassroom(Transform exam)
    {
        foreach (float z in new[] { -2.3f, 0.55f, 3.45f, 6.3f })
        {
            DeskPart(exam, "WindowGlass", new Vector3(-3f, 1.85f, z), new Vector3(0.035f, 1.7f, 2.3f), new Color(0.60f, 0.78f, 0.84f), true);
            foreach (float dz in new[] { -1.15f, 0f, 1.15f })
                DeskPart(exam, "WindowMullion", new Vector3(-3.12f, 1.85f, z + dz), new Vector3(0.05f, 1.7f, 0.04f), EscapePalette.Cabinet, false);
            foreach (float y in new[] { 1f, 2.7f })
                DeskPart(exam, "WindowFrame", new Vector3(-3.12f, y, z), new Vector3(0.05f, 0.04f, 2.35f), EscapePalette.Cabinet, false);
        }
        DeskPart(exam, "WallDado", new Vector3(-3.12f, 0.5f, 2f), new Vector3(0.035f, 1f, 11.8f), new Color(0.52f, 0.65f, 0.59f), false);
        DeskPart(exam, "ChalkTray", new Vector3(-7.5f, 1.39f, 7.76f), new Vector3(4.1f, 0.04f, 0.13f), EscapePalette.Cabinet, false);
        MakeSign(exam, "ExamRoomSign", "201 考场   保持安静", new Vector3(-7.5f, 3.05f, 7.75f), Quaternion.identity);
        foreach (float x in new[] { -9.5f, -5.5f })
            foreach (float z in new[] { -1f, 3f, 6.5f }) AddRoomLight(exam, new Vector3(x, 3.25f, z), 4.5f);
    }

    static void CreateCabinet(Transform root, Vector3 pos)
    {
        var cabinet = new GameObject("Cabinet").transform;
        cabinet.SetParent(root, false);
        cabinet.localPosition = pos - Vector3.up;
        DeskPart(cabinet, "CabinetBody", new Vector3(0f, 1f, 0f), new Vector3(1.1f, 2f, 0.7f), EscapePalette.Cabinet);
        foreach (float x in new[] { -0.27f, 0.27f })
        {
            DeskPart(cabinet, "CabinetDoor", new Vector3(x, 1f, -0.36f), new Vector3(0.52f, 1.9f, 0.025f), new Color(0.58f, 0.63f, 0.61f), false);
            DeskPart(cabinet, "CabinetHandle", new Vector3(x * 0.3f, 1f, -0.39f), new Vector3(0.025f, 0.18f, 0.035f), Color.gray, false);
        }
        cabinet.gameObject.AddComponent<CoverTag>().label = "储物柜";
    }
    public static GameObject CreateTeacher(Transform root, string name, Vector3 pos, Quaternion rot,
        Vector3[] route, float patrolSpeed, float chaseSpeed, ScrutinyAura.Role role,
        float viewDist, float viewAngle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        go.transform.rotation = rot;

        Color body = role == ScrutinyAura.Role.Parent ? EscapePalette.Parent
            : (role == ScrutinyAura.Role.Patrol ? EscapePalette.Patrol : EscapePalette.Teacher);
        DeskPart(go.transform, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.44f, 0.56f, 0.25f), body, false);
        LevelBuildUtil.Make(PrimitiveType.Sphere, go.transform, "Head", new Vector3(0f, 1.61f, 0f),
            new Vector3(0.26f, 0.28f, 0.25f), Quaternion.identity, new Color(0.77f, 0.60f, 0.46f), "NPCSkin", false);
        foreach (float x in new[] { -0.115f, 0.115f })
            DeskPart(go.transform, "Leg", new Vector3(x, 0.45f, 0f), new Vector3(0.17f, 0.9f, 0.20f), new Color(0.19f, 0.22f, 0.26f), false);
        foreach (float x in new[] { -0.285f, 0.285f })
            DeskPart(go.transform, "Arm", new Vector3(x, 1.12f, 0f), new Vector3(0.12f, 0.62f, 0.15f), body, false);
        var col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0.875f, 0f);
        col.height = 1.75f;
        col.radius = 0.27f;

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = Mathf.Min(viewDist,6.5f);
        cone.detectionDelay = 0.65f;
        cone.viewAngle = Mathf.Min(viewAngle,52f);
        cone.eyeHeight = 1.62f;
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
        agent.patrolSpeed = Mathf.Min(patrolSpeed,1.1f);
        agent.chaseSpeed = Mathf.Min(chaseSpeed,3.3f);
        agent.randomPatrol = false;
        agent.waitAtPoint = 1.2f;
        agent.loseSightReturnSeconds = 2.2f;
        agent.catchDistance = 0.75f;

        var badge=LevelBuildUtil.Make(PrimitiveType.Cube,go.transform,"WarningBadge",
            new Vector3(0f,1.3f,0.145f),new Vector3(0.11f,0.055f,0.025f),Quaternion.identity,
            new Color(1f,0.2f,0.04f),"WarningBadge",false,true,0.15f);
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
        follow.firstPersonOnFoot = true;
        follow.firstPersonFov = 68f;
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

        if (systems.GetComponent<CampusHUD>() == null)
            systems.AddComponent<CampusHUD>();
        flow.BindTeachers();
        homeFlow.BindDetectors();
        PauseMenu.Ensure();
        follow.Snap();
    }
}

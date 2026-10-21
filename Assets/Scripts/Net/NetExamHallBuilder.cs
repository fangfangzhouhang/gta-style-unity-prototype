using UnityEngine;

/// <summary>
/// 生成 4 间分隔考场 + 共享汇合黄圈。供联机骨架验收 Host/Client。
/// </summary>
public static class NetExamHallBuilder
{
    public static void Build(bool forNetwork = true)
    {
        LevelBuildUtil.DestroyByName("NetLevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("NetLevelRoot").transform;

        // Shared ground strip
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Ground",
            new Vector3(0f, -0.1f, 0f), new Vector3(90f, 0.2f, 50f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);

        var spawns = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            float x = -30f + i * 20f;
            spawns[i] = BuildRoom(root, i, new Vector3(x, 0f, 0f));
        }

        // Meetup plaza on +Z
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "MeetupPad",
            new Vector3(0f, -0.05f, 22f), new Vector3(18f, 0.1f, 12f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", true, true, 0.45f);

        var meetupMarker = new GameObject("MeetupPoint");
        meetupMarker.transform.SetParent(root, false);
        meetupMarker.transform.position = new Vector3(0f, 1f, 22f);

        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, "MeetupBeacon",
            new Vector3(0f, 2f, 22f), new Vector3(2.2f, 2.5f, 2.2f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", false, true, 0.6f);

        var assignerGo = new GameObject("ExamRoomAssigner");
        assignerGo.transform.SetParent(root, false);
        ExamRoomAssigner assigner = assignerGo.AddComponent<ExamRoomAssigner>();
        assigner.SetSpawns(spawns);

        var meetupGo = new GameObject("MeetupZone");
        meetupGo.transform.SetParent(root, false);
        MeetupZone zone = meetupGo.AddComponent<MeetupZone>();
        zone.SetMeetupPoint(meetupMarker.transform);
        zone.gatherRadius = 7f;

        // Offline preview only when explicitly offline build
        if (!forNetwork)
        {
            Vector3 spawn = spawns[0].position;
            GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, true);
            ThirdPersonCamera orbit = cam.GetComponent<ThirdPersonCamera>();
            if (orbit != null)
                orbit.target = player.transform;
        }

        // Net exit triggers already on rooms; wire HUD
        EscapeNetHud.Ensure().SetObjective("分考场潜行", "走进自己考场的绿门 → 汇合点");
    }

    static Transform BuildRoom(Transform root, int index, Vector3 origin)
    {
        var room = new GameObject("ExamRoom_" + (index + 1)).transform;
        room.SetParent(root, false);
        room.position = origin;

        LevelBuildUtil.Make(PrimitiveType.Cube, room, "Floor",
            new Vector3(0f, -0.05f, 4f), new Vector3(16f, 0.1f, 14f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);

        // Walls
        MakeWall(room, "WallBack", new Vector3(0f, 1.6f, -2.5f), new Vector3(16f, 3.2f, 0.35f));
        MakeWall(room, "WallFront", new Vector3(0f, 1.6f, 10.5f), new Vector3(16f, 3.2f, 0.35f));
        MakeWall(room, "WallLeft", new Vector3(-8f, 1.6f, 4f), new Vector3(0.35f, 3.2f, 13.2f));
        MakeWall(room, "WallRight", new Vector3(8f, 1.6f, 4f), new Vector3(0.35f, 3.2f, 13.2f));

        // Desks cover
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Vector3 p = new Vector3(-4f + c * 4f, 0.4f, 1f + r * 2.5f);
                GameObject desk = LevelBuildUtil.Make(PrimitiveType.Cube, room, "Desk_" + r + "_" + c,
                    p, new Vector3(1.6f, 0.8f, 0.9f), Quaternion.identity,
                    EscapePalette.Desk, "Crate", true);
                if (desk != null)
                    desk.AddComponent<CoverTag>();
            }
        }

        // Simple teacher (local AI, not networked — skeleton)
        Transform[] route =
        {
            MakeWp(room, "WP_A", new Vector3(-3f, 0f, 7f)),
            MakeWp(room, "WP_B", new Vector3(3f, 0f, 7f)),
            MakeWp(room, "WP_C", new Vector3(0f, 0f, 8.5f))
        };
        CreateTeacher(room, new Vector3(0f, 0f, 7.5f), route);

        // Green exit toward meetup (+Z gap in front wall — use trigger outside)
        LevelBuildUtil.Make(PrimitiveType.Cube, room, "ExitDoor",
            new Vector3(0f, 1.2f, 10.3f), new Vector3(2.2f, 2.4f, 0.2f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", false, true, 0.55f);

        var exit = new GameObject("NetExamExit");
        exit.transform.SetParent(room, false);
        exit.transform.position = new Vector3(0f, 1f, 11.2f);
        var box = exit.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(2.4f, 2.2f, 2f);
        exit.AddComponent<NetExamExitTrigger>();

        var spawn = new GameObject("Spawn");
        spawn.transform.SetParent(room, false);
        spawn.transform.position = origin + new Vector3(0f, 1.02f, -1.2f);
        spawn.transform.rotation = Quaternion.identity;

        // Label pillar
        LevelBuildUtil.Make(PrimitiveType.Cube, room, "RoomLabel",
            new Vector3(0f, 3.2f, 4f), new Vector3(3f, 0.4f, 0.3f), Quaternion.identity,
            EscapePalette.Blackboard, "BuildingC", false);

        return spawn.transform;
    }

    static void MakeWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        LevelBuildUtil.Make(PrimitiveType.Cube, parent, name, pos, scale, Quaternion.identity,
            EscapePalette.Wall, "BuildingA", true);
    }

    static Transform MakeWp(Transform parent, string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    static void CreateTeacher(Transform parent, Vector3 localPos, Transform[] route)
    {
        var go = new GameObject("Teacher");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        LevelBuildUtil.Make(PrimitiveType.Capsule, go.transform, "Body",
            Vector3.zero, new Vector3(0.7f, 0.9f, 0.7f), Quaternion.identity,
            EscapePalette.Teacher, "CarBody", false);

        var cone = go.AddComponent<VisionConeDetector>();
        cone.viewDistance = 7f;
        cone.viewAngle = 50f;

        var agent = go.AddComponent<TeacherAgent>();
        agent.waypoints = route;
        agent.patrolSpeed = 1.6f;
        agent.chaseSpeed = 4.2f;
        agent.randomPatrol = true;
    }
}

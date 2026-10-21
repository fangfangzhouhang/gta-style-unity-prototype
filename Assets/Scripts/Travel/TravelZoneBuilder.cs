using UnityEngine;

/// <summary>
/// 生成旅行任务占位区：开阔地面 + 至少 3 个中国地标主题黄圈。
/// </summary>
public static class TravelZoneBuilder
{
    public static void Build()
    {
        LevelBuildUtil.DestroyByName("LevelRoot");
        LevelBuildUtil.DestroyByName("Player");

        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("LevelRoot").transform;

        LevelBuildUtil.Make(PrimitiveType.Cube, root, "TravelGround",
            new Vector3(0f, -0.1f, 18f), new Vector3(48f, 0.2f, 48f), Quaternion.identity,
            EscapePalette.TravelGround, "Ground", true);

        // 简单地标占位块，增加可读性
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Landmark_WestLake",
            new Vector3(-10f, 1.2f, 16f), new Vector3(3f, 2.4f, 3f), Quaternion.identity,
            new Color(0.25f, 0.55f, 0.75f), "Glass", true, true, 0.35f);
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Landmark_Wall",
            new Vector3(12f, 1.5f, 20f), new Vector3(5f, 3f, 1.2f), Quaternion.identity,
            new Color(0.55f, 0.42f, 0.3f), "BuildingB", true);
        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, "Landmark_Pine",
            new Vector3(0f, 2f, 34f), new Vector3(1.2f, 4f, 1.2f), Quaternion.identity,
            new Color(0.2f, 0.45f, 0.22f), "BuildingA", true);

        var points = new TravelMissionPoint[TravelMissionCatalog.Missions.Length];
        for (int i = 0; i < TravelMissionCatalog.Missions.Length; i++)
        {
            var entry = TravelMissionCatalog.Missions[i];
            points[i] = CreateMissionRing(root, entry);
        }

        Vector3 spawn = new Vector3(0f, 1.02f, 0f);
        GameObject player = LevelBuildUtil.CreatePlayer(root, spawn, false);

        Wire(player, cam, points);
    }

    static TravelMissionPoint CreateMissionRing(Transform root, TravelMissionCatalog.Entry entry)
    {
        var go = new GameObject("Mission_" + entry.id);
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(entry.x, 0.05f, entry.z);

        LevelBuildUtil.Make(PrimitiveType.Cylinder, go.transform, "Ring",
            new Vector3(0f, 0.05f, 0f), new Vector3(3.2f, 0.08f, 3.2f), Quaternion.identity,
            CityPalette.Checkpoint, "Checkpoint", false, true, 0.5f);

        var beacon = LevelBuildUtil.Make(PrimitiveType.Cylinder, go.transform, "Beacon",
            new Vector3(0f, 4f, 0f), new Vector3(0.55f, 8f, 0.55f), Quaternion.identity,
            CityPalette.Checkpoint, "Checkpoint", false, true, 0.65f);

        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2.2f;
        col.center = new Vector3(0f, 1f, 0f);

        var point = go.AddComponent<TravelMissionPoint>();
        point.missionId = entry.id;
        point.title = entry.title;
        point.beacon = beacon.transform;
        return point;
    }

    static void Wire(GameObject player, Camera cam, TravelMissionPoint[] points)
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

        if (systems.GetComponent<TravelZoneBootstrap>() == null)
            systems.AddComponent<TravelZoneBootstrap>();

        TravelGameHUD hud = systems.GetComponent<TravelGameHUD>();
        if (hud == null)
            hud = systems.AddComponent<TravelGameHUD>();
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        TravelMissionController ctrl = systems.GetComponent<TravelMissionController>();
        if (ctrl == null)
            ctrl = systems.AddComponent<TravelMissionController>();
        ctrl.hud = hud;
        ctrl.points = points;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
                points[i].controller = ctrl;
        }

        StealthGameHUD stealth = systems.GetComponent<StealthGameHUD>();
        if (stealth != null)
            LevelBuildUtil.DestroyObject(stealth);
        MissionHUD legacy = systems.GetComponent<MissionHUD>();
        if (legacy != null)
            LevelBuildUtil.DestroyObject(legacy);
        TransitGameHUD transitHud = systems.GetComponent<TransitGameHUD>();
        if (transitHud != null)
            LevelBuildUtil.DestroyObject(transitHud);

        follow.Snap();
    }
}

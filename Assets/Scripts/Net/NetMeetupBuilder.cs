using UnityEngine;

/// <summary>
/// 独立汇合场景占位：黄圈 + 提示。
/// </summary>
public static class NetMeetupBuilder
{
    public static void Build(bool forNetwork = true)
    {
        LevelBuildUtil.DestroyByName("MeetupRoot");
        LevelBuildUtil.EnsureSun();
        Camera cam = LevelBuildUtil.EnsureCamera();

        var root = new GameObject("MeetupRoot").transform;
        LevelBuildUtil.Make(PrimitiveType.Cube, root, "Ground",
            new Vector3(0f, -0.1f, 0f), new Vector3(30f, 0.2f, 30f), Quaternion.identity,
            EscapePalette.Floor, "Sidewalk", true);

        LevelBuildUtil.Make(PrimitiveType.Cylinder, root, "MeetupBeacon",
            new Vector3(0f, 2f, 0f), new Vector3(3f, 2.5f, 3f), Quaternion.identity,
            EscapePalette.Exit, "Checkpoint", false, true, 0.55f);

        var point = new GameObject("MeetupPoint");
        point.transform.SetParent(root, false);
        point.transform.position = new Vector3(0f, 1f, 0f);

        var zoneGo = new GameObject("MeetupZone");
        zoneGo.transform.SetParent(root, false);
        MeetupZone zone = zoneGo.AddComponent<MeetupZone>();
        zone.SetMeetupPoint(point.transform);

        if (!forNetwork)
        {
            GameObject player = LevelBuildUtil.CreatePlayer(root, new Vector3(0f, 1.02f, -4f), false);
            ThirdPersonCamera orbit = cam.GetComponent<ThirdPersonCamera>();
            if (orbit != null)
                orbit.target = player.transform;
        }
    }
}

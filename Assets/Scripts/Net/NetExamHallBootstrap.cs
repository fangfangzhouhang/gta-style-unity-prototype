using UnityEngine;

/// <summary>
/// 联机考场入口。空场景时生成多考场 + 汇合点骨架。
/// </summary>
public class NetExamHallBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        EscapeNetHud.Ensure();

        if (UnityCompat.FindOne<EscapeNetworkBootstrap>() == null)
        {
            var net = new GameObject("EscapeNetworkBootstrap");
            var boot = net.AddComponent<EscapeNetworkBootstrap>();
            boot.showMenu = false;
            boot.loadExamOnHost = false;
        }

        if (GameObject.Find("NetLevelRoot") == null)
            NetExamHallBuilder.Build(forNetwork: true);

        if (OfflineFallback.IsNetworkSessionActive())
        {
            // Remove offline preview pawn if scene was saved with one
            GameObject stray = GameObject.Find("Player");
            if (stray != null && stray.GetComponent<NetworkPlayer>() == null)
                Destroy(stray);
        }
        else if (GameObject.Find("Player") == null)
        {
            // Pure offline preview of the net layout
            ExamRoomAssigner assigner = UnityCompat.FindOne<ExamRoomAssigner>();
            Vector3 spawn = new Vector3(-30f, 1.02f, -1.2f);
            if (assigner != null && assigner.roomSpawns != null && assigner.roomSpawns.Length > 0
                && assigner.roomSpawns[0] != null)
                spawn = assigner.roomSpawns[0].position;
            Transform parent = GameObject.Find("NetLevelRoot") != null
                ? GameObject.Find("NetLevelRoot").transform
                : null;
            GameObject player = LevelBuildUtil.CreatePlayer(parent, spawn, true);
            Camera cam = Camera.main;
            if (cam != null)
            {
                ThirdPersonCamera orbit = cam.GetComponent<ThirdPersonCamera>();
                if (orbit != null)
                    orbit.target = player.transform;
            }
        }
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
        EscapeNetHud.Ensure().SetObjective("分考场潜行逃出", "绿门逃出后前往黄色汇合点");
        EscapeNetHud.Ensure().SetStatus(
            OfflineFallback.IsNetworkSessionActive() ? "联机会话中" : "离线预览（无 NetworkManager 会话）");
    }
}

using UnityEngine;

/// <summary>
/// 联机大厅入口：确保 Network 菜单与 HUD。
/// </summary>
public class NetLobbyBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        EscapeNetHud.Ensure().SetObjective("联机大厅", "Host 开房或 Client 输入局域网地址加入");
        EscapeNetHud.Ensure().SetStatus("骨架模式 · 等待 Package Manager 解析 Netcode");

        if (GetComponent<EscapeNetworkBootstrap>() == null)
            gameObject.AddComponent<EscapeNetworkBootstrap>();

        LevelBuildUtil.EnsureSun();
        LevelBuildUtil.EnsureCamera();

        if (GameObject.Find("LobbyFloor") == null)
        {
            LevelBuildUtil.Make(PrimitiveType.Cube, null, "LobbyFloor",
                new Vector3(0f, -0.1f, 0f), new Vector3(20f, 0.2f, 20f), Quaternion.identity,
                EscapePalette.Floor, "Sidewalk", true);
        }
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

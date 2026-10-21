using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 单机 / 联机判定。无 NetworkManager 或未开局时走原单机流程。
/// </summary>
public static class OfflineFallback
{
    public static bool IsNetworkSessionActive()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
            return false;
        return nm.IsListening || nm.IsConnectedClient || nm.IsServer || nm.IsClient;
    }

    public static bool HasNetworkManager()
    {
        return NetworkManager.Singleton != null;
    }

    /// <summary>联机会话中应抑制「逃出学校→HubTown」单机跳转，改走汇合点。</summary>
    public static bool ShouldUseMeetupInsteadOfHub()
    {
        return IsNetworkSessionActive();
    }
}

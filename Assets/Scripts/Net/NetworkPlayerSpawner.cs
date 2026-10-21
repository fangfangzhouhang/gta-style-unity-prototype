using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 确保 NetworkManager 使用玩家模板；连接后由 NGO 自动生成 PlayerPrefab。
/// </summary>
public class NetworkPlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    public void EnsurePlayerPrefabRegistered(NetworkManager nm)
    {
        if (nm == null)
            return;

        if (playerPrefab == null)
            playerPrefab = NetPlayerTemplateFactory.EnsureTemplate();

        if (playerPrefab.GetComponent<NetworkObject>() == null)
            playerPrefab.AddComponent<NetworkObject>();
        if (playerPrefab.GetComponent<NetworkPlayer>() == null)
            playerPrefab.AddComponent<NetworkPlayer>();
        if (playerPrefab.GetComponent<EscapeNetProgress>() == null)
            playerPrefab.AddComponent<EscapeNetProgress>();

        nm.NetworkConfig.PlayerPrefab = playerPrefab;

        try
        {
            nm.AddNetworkPrefab(playerPrefab);
        }
        catch (System.Exception ex)
        {
            // Already registered is fine
            Debug.Log("[EscapeNet] AddNetworkPrefab: " + ex.Message);
        }
    }

    public void SpawnForClient(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer)
            return;
        EnsurePlayerPrefabRegistered(nm);

        GameObject go = Instantiate(playerPrefab);
        go.SetActive(true);
        NetworkObject no = go.GetComponent<NetworkObject>();
        no.SpawnAsPlayerObject(clientId, true);
    }
}

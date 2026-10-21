using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 按 clientId 分配不同考场座位 / 房间 spawn（默认 4 个考场点）。
/// </summary>
public class ExamRoomAssigner : MonoBehaviour
{
    public Transform[] roomSpawns = new Transform[0];
    public int roomCount = 4;

    readonly Dictionary<ulong, int> assigned = new Dictionary<ulong, int>();

    public static ExamRoomAssigner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetSpawns(Transform[] spawns)
    {
        roomSpawns = spawns ?? new Transform[0];
        roomCount = Mathf.Max(1, roomSpawns.Length);
    }

    public Transform GetSpawnForClient(ulong clientId)
    {
        if (roomSpawns == null || roomSpawns.Length == 0)
            return null;

        int index;
        if (!assigned.TryGetValue(clientId, out index))
        {
            index = (int)(clientId % (ulong)roomSpawns.Length);
            // Prefer empty rooms in order for first few clients
            for (int i = 0; i < roomSpawns.Length; i++)
            {
                bool taken = false;
                foreach (var kv in assigned)
                {
                    if (kv.Value == i)
                    {
                        taken = true;
                        break;
                    }
                }
                if (!taken)
                {
                    index = i;
                    break;
                }
            }
            assigned[clientId] = index;
        }

        if (index < 0 || index >= roomSpawns.Length)
            index = 0;
        return roomSpawns[index];
    }

    public void NotifyAssigned(ulong clientId, Transform spot)
    {
        int idx = -1;
        if (roomSpawns != null)
        {
            for (int i = 0; i < roomSpawns.Length; i++)
            {
                if (roomSpawns[i] == spot)
                {
                    idx = i;
                    break;
                }
            }
        }
        EscapeNetHud.Ensure().SetStatus(
            "你被分配到考场 " + (idx >= 0 ? (idx + 1).ToString() : "?") +
            "（clientId=" + clientId + "）");
    }

    public int GetAssignedRoomIndex(ulong clientId)
    {
        int index;
        if (assigned.TryGetValue(clientId, out index))
            return index;
        return -1;
    }
}

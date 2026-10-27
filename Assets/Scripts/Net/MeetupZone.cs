using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 逃出成功后传送到汇合点；其他人到达后提示「队友已汇合」。
/// </summary>
public class MeetupZone : MonoBehaviour
{
    public Transform meetupPoint;
    public float gatherRadius = 6f;

    readonly HashSet<ulong> greeted = new HashSet<ulong>();
    bool announcedTeam;

    public void SetMeetupPoint(Transform point)
    {
        meetupPoint = point;
    }

    public void TeleportOwnerToMeetup(EscapeNetProgress progress)
    {
        if (progress == null || meetupPoint == null)
            return;
        NetworkPlayer player = progress.GetComponent<NetworkPlayer>();
        if (player == null || !player.IsOwner)
            return;

        Vector3 offset = new Vector3(
            (progress.OwnerClientId % 4) * 1.2f - 1.8f,
            0f,
            0f);
        player.TeleportLocal(meetupPoint.position + offset, meetupPoint.rotation);
        progress.MarkAtMeetupLocal();
        EscapeNetHud.Ensure().SetStatus("你已到达汇合点，等待队友…");
        EscapeNetHud.Ensure().SetObjective("汇合点", "其他玩家逃出后会到这里见面");
    }

    void Update()
    {
        if (!OfflineFallback.IsNetworkSessionActive())
            return;
        if (meetupPoint == null)
            return;

        EscapeNetProgress[] all = UnityCompat.FindAll<EscapeNetProgress>();
        int atMeetup = 0;
        int escaped = 0;
        int total = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null || !all[i].IsSpawned)
                continue;
            total++;
            if (all[i].HasEscaped.Value)
                escaped++;
            if (all[i].AtMeetup.Value)
                atMeetup++;
        }

        if (atMeetup >= 2 && !announcedTeam)
        {
            announcedTeam = true;
            EscapeNetHud.Ensure().ShowBanner("队友已汇合", "你们在汇合点见面了（骨架演示）");
        }
        else if (atMeetup == 1 && escaped >= 1)
        {
            EscapeNetHud.Ensure().SetDetail("已有 " + atMeetup + " 人在汇合点（共 " + total + " 人）");
        }

        // Also detect by proximity for players already teleported
        NetworkPlayer[] players = UnityCompat.FindAll<NetworkPlayer>();
        int near = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;
            if (Vector3.Distance(players[i].transform.position, meetupPoint.position) <= gatherRadius)
                near++;
        }
        if (near >= 2 && !announcedTeam)
        {
            announcedTeam = true;
            EscapeNetHud.Ensure().ShowBanner("队友已汇合", "距离判定：多名玩家已在汇合圈内");
        }
    }
}

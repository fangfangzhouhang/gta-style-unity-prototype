using UnityEngine;

/// <summary>
/// 联机考场绿门：Owner 玩家进入后标记逃出并传送汇合点。
/// 离线预览时也传送到 MeetupPoint。
/// </summary>
public class NetExamExitTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        NetworkPlayer netPlayer = other.GetComponentInParent<NetworkPlayer>();
        if (netPlayer != null)
        {
            if (!netPlayer.IsOwner)
                return;
            EscapeNetProgress progress = netPlayer.GetComponent<EscapeNetProgress>();
            if (progress != null)
                progress.MarkEscapedLocal();
            return;
        }

        // Offline fallback preview player
        if (!OfflineFallback.IsNetworkSessionActive())
        {
            ThirdPersonController motor = other.GetComponentInParent<ThirdPersonController>();
            if (motor == null)
                return;
            MeetupZone zone = UnityCompat.FindOne<MeetupZone>();
            Transform meetup = zone != null ? zone.meetupPoint : null;
            if (meetup == null)
            {
                GameObject mp = GameObject.Find("MeetupPoint");
                if (mp != null)
                    meetup = mp.transform;
            }
            if (meetup != null)
            {
                CharacterController body = motor.GetComponent<CharacterController>();
                if (body != null)
                    body.enabled = false;
                motor.transform.position = meetup.position;
                if (body != null)
                    body.enabled = true;
                GameProgress.Ensure().MarkEscapedSchool();
                EscapeNetHud.Ensure().ShowBanner("逃出考场", "离线预览：已到汇合点");
            }
        }
    }
}

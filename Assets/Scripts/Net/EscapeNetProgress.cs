using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 网络侧「是否已逃出学校/考场」进度。挂在玩家 NetworkObject 上。
/// </summary>
public class EscapeNetProgress : NetworkBehaviour
{
    public NetworkVariable<bool> HasEscaped = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> AtMeetup = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public void MarkEscapedLocal()
    {
        if (!IsOwner)
            return;
        if (HasEscaped.Value)
            return;
        HasEscaped.Value = true;
        GameProgress.Ensure().MarkEscapedSchool();
        MeetupZone zone = UnityCompat.FindOne<MeetupZone>();
        if (zone != null)
            zone.TeleportOwnerToMeetup(this);
        else
            Debug.Log("[EscapeNet] 已标记逃出，但场景中无 MeetupZone。");
    }

    public void MarkAtMeetupLocal()
    {
        if (!IsOwner)
            return;
        AtMeetup.Value = true;
    }
}

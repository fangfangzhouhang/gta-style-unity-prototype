using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 最小联机玩家：Owner 写位置/朝向，远端插值；并携带「是否已逃出」同步。
/// 也可在预制体上额外挂 NetworkTransform，本类不依赖它。
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    public float remoteLerp = 12f;

    NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    ThirdPersonController motor;
    CharacterController body;
    bool assignedRoom;

    public ulong OwnerClientIdSafe
    {
        get { return OwnerClientId; }
    }

    void Awake()
    {
        motor = GetComponent<ThirdPersonController>();
        body = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            if (motor != null)
                motor.controlEnabled = true;
            Camera cam = Camera.main;
            if (cam != null)
            {
                ThirdPersonCamera orbit = cam.GetComponent<ThirdPersonCamera>();
                if (orbit != null)
                    orbit.target = transform;
            }
            TryAssignRoom();
        }
        else
        {
            if (motor != null)
                motor.controlEnabled = false;
            if (body != null)
                body.enabled = false;
        }

        EscapeNetHud.Ensure().RegisterPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        EscapeNetHud hud = EscapeNetHud.Instance;
        if (hud != null)
            hud.UnregisterPlayer(this);
        base.OnNetworkDespawn();
    }

    void TryAssignRoom()
    {
        if (assignedRoom)
            return;
        ExamRoomAssigner assigner = UnityCompat.FindOne<ExamRoomAssigner>();
        if (assigner == null)
            return;
        Transform spot = assigner.GetSpawnForClient(OwnerClientId);
        if (spot == null)
            return;
        TeleportLocal(spot.position, spot.rotation);
        assignedRoom = true;
        assigner.NotifyAssigned(OwnerClientId, spot);
    }

    void Update()
    {
        if (!IsSpawned)
            return;

        if (IsOwner)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation;
            if (!assignedRoom)
                TryAssignRoom();
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position, netPosition.Value, Time.deltaTime * remoteLerp);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, netRotation.Value, Time.deltaTime * remoteLerp);
        }
    }

    public void TeleportLocal(Vector3 position, Quaternion rotation)
    {
        if (body != null)
            body.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        if (body != null)
            body.enabled = IsOwner;
        if (IsOwner)
        {
            netPosition.Value = position;
            netRotation.Value = rotation;
        }
        ThirdPersonCamera cam = UnityCompat.FindOne<ThirdPersonCamera>();
        if (cam != null && IsOwner)
            cam.Snap();
    }
}

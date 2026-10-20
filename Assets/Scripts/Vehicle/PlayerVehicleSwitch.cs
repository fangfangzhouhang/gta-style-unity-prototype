using UnityEngine;

/// <summary>
/// 靠近车辆按 F 上车，驾驶中再按 F 下车。
/// 上车会关掉角色控制器，把人放到座位上并隐藏身体，镜头改跟车辆。
/// </summary>
public class PlayerVehicleSwitch : MonoBehaviour
{
    public VehicleController vehicle;
    public ThirdPersonController player;
    public ThirdPersonCamera cameraRig;
    public Transform seat;
    public Transform enterHint;
    public Renderer[] hideWhenDriving;

    [Tooltip("角色中心到车碰撞体最近点小于该距离时可以上车。")]
    public float enterDistance = 2.5f;

    public bool IsDriving { get; private set; }

    float switchLockout;
    Transform returnParent;
    float hintBaseY = 2.55f;

    void Awake()
    {
        if (player == null)
            player = GetComponent<ThirdPersonController>();

        if (vehicle == null)
            vehicle = UnityCompat.FindOne<VehicleController>();

        if (cameraRig == null)
            cameraRig = UnityCompat.FindOne<ThirdPersonCamera>();

        if (vehicle != null)
        {
            if (seat == null)
                seat = vehicle.transform.Find("Seat");
            if (enterHint == null)
            {
                Transform hint = vehicle.transform.Find("EnterHint");
                if (hint != null)
                    enterHint = hint;
            }
        }

        if ((hideWhenDriving == null || hideWhenDriving.Length == 0) && player != null)
            hideWhenDriving = player.GetComponentsInChildren<Renderer>();

        if (enterHint != null)
            hintBaseY = enterHint.localPosition.y;
    }

    void Update()
    {
        if (switchLockout > 0f)
            switchLockout -= Time.deltaTime;

        BobHint();

        if (switchLockout > 0f)
            return;

        if (!Input.GetKeyDown(KeyCode.F))
            return;

        if (IsDriving)
            Exit();
        else if (IsCloseEnough())
            Enter();
    }

    public bool IsCloseEnough()
    {
        if (IsDriving || vehicle == null)
            return false;

        Collider col = vehicle.GetComponent<Collider>();
        Vector3 origin = transform.position;
        if (col != null)
        {
            Vector3 closest = col.bounds.ClosestPoint(origin);
            return Vector3.Distance(origin, closest) <= enterDistance;
        }

        Vector3 a = origin;
        Vector3 b = vehicle.transform.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b) <= enterDistance + 1.5f;
    }

    void Enter()
    {
        if (vehicle == null || player == null)
            return;

        IsDriving = true;
        switchLockout = 0.35f;

        player.controlEnabled = false;
        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = false;

        SetBodyVisible(false);
        if (enterHint != null)
            enterHint.gameObject.SetActive(false);

        returnParent = transform.parent;
        Transform sit = seat != null ? seat : vehicle.transform;
        transform.SetParent(sit, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        vehicle.controlEnabled = true;

        if (cameraRig != null)
        {
            cameraRig.SetFollow(
                vehicle.transform,
                ThirdPersonCamera.DrivingOffset,
                ThirdPersonCamera.DrivingDistance,
                true);
        }
    }

    void Exit()
    {
        if (vehicle == null || player == null)
            return;

        IsDriving = false;
        switchLockout = 0.35f;
        vehicle.controlEnabled = false;

        Vector3 drop = FindExitPosition();
        transform.SetParent(returnParent, true);
        transform.position = drop;
        transform.rotation = Quaternion.Euler(0f, vehicle.transform.eulerAngles.y, 0f);

        SetBodyVisible(true);
        if (enterHint != null)
            enterHint.gameObject.SetActive(true);

        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = true;

        player.ResetVerticalSpeed();
        player.controlEnabled = true;

        if (cameraRig != null)
        {
            cameraRig.SetFollow(
                transform,
                ThirdPersonCamera.OnFootOffset,
                ThirdPersonCamera.OnFootDistance,
                true);
        }
    }

    Vector3 FindExitPosition()
    {
        Vector3[] localOffsets =
        {
            new Vector3(-2.5f, 0f, 0.2f),
            new Vector3(2.5f, 0f, 0.2f),
            new Vector3(0f, 0f, -3.3f),
            new Vector3(0f, 0f, 3.3f)
        };

        for (int i = 0; i < localOffsets.Length; i++)
        {
            Vector3 world = vehicle.transform.TransformPoint(localOffsets[i]);
            world.y = 1.05f;
            if (IsClear(world))
                return world;
        }

        Vector3 fallback = vehicle.transform.position - vehicle.transform.right * 2.5f;
        fallback.y = 1.05f;
        return fallback;
    }

    static bool IsClear(Vector3 center)
    {
        // 胶囊底部抬离地面，避免下车点被地面误判成挡住。
        Vector3 raised = new Vector3(center.x, 1.35f, center.z);
        Vector3 a = raised + Vector3.up * 0.45f;
        Vector3 b = raised - Vector3.up * 0.35f;
        return !Physics.CheckCapsule(a, b, 0.32f, ~0, QueryTriggerInteraction.Ignore);
    }

    void SetBodyVisible(bool visible)
    {
        if (hideWhenDriving == null)
            return;

        for (int i = 0; i < hideWhenDriving.Length; i++)
        {
            if (hideWhenDriving[i] != null)
                hideWhenDriving[i].enabled = visible;
        }
    }

    void BobHint()
    {
        if (enterHint == null || !enterHint.gameObject.activeSelf)
            return;

        Vector3 pos = enterHint.localPosition;
        pos.y = hintBaseY + Mathf.Sin(Time.time * 3.2f) * 0.18f;
        enterHint.localPosition = pos;
    }
}

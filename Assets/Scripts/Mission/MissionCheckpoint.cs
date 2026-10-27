using UnityEngine;

/// <summary>
/// 黄色检查点。玩家或车辆进入触发器后标记完成。
/// 光柱本身没有碰撞，车可以直接开过去。
/// </summary>
public class MissionCheckpoint : MonoBehaviour
{
    public static MissionCheckpoint Active { get; private set; }

    public Transform beacon;
    public string objective = "开车前往黄色光柱";

    public bool IsComplete { get; private set; }

    public Vector3 BeaconWorldPosition
    {
        get { return beacon != null ? beacon.position : transform.position + Vector3.up * 6f; }
    }

    void OnEnable()
    {
        Active = this;
    }

    void Awake()
    {
        if (beacon == null)
            beacon = transform.Find("Beacon");
    }

    void Update()
    {
        if (beacon == null)
            return;

        beacon.Rotate(0f, 28f * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || other == null)
            return;

        bool player = other.GetComponentInParent<ThirdPersonController>() != null;
        bool car = other.GetComponentInParent<VehicleController>() != null;
        if (!player && !car)
            return;

        IsComplete = true;
    }
}

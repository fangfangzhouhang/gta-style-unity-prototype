using UnityEngine;

/// <summary>
/// 旅行任务黄圈。玩家进入触发器后标记完成并通知控制器。
/// </summary>
public class TravelMissionPoint : MonoBehaviour
{
    public string missionId;
    public string title = "旅行点";
    public Transform beacon;
    public TravelMissionController controller;

    public bool IsComplete { get; private set; }

    void Awake()
    {
        if (beacon == null)
            beacon = transform.Find("Beacon");
        if (controller == null)
            controller = UnityCompat.FindOne<TravelMissionController>();
    }

    void Update()
    {
        if (beacon != null)
            beacon.Rotate(0f, 28f * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null &&
            other.GetComponentInParent<VehicleController>() == null)
            return;

        IsComplete = true;
        if (controller == null)
            controller = UnityCompat.FindOne<TravelMissionController>();
        if (controller != null)
            controller.NotifyMissionComplete(this);
    }
}

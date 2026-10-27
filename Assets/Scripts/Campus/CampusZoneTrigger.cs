using UnityEngine;

/// <summary>
/// 进入某区域时更新 CampusFlow 的检查点 / 分区（同场景）。
/// </summary>
public class CampusZoneTrigger : MonoBehaviour
{
    public CampusFlowController.Zone zone = CampusFlowController.Zone.School;
    public CampusFlowController flow;

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (flow == null)
            flow = UnityCompat.FindOne<CampusFlowController>();
        if (flow == null)
            return;
        if (zone == CampusFlowController.Zone.Home)
            flow.NotifyEnteredHome();
        else if (zone == CampusFlowController.Zone.Hub)
            flow.NotifyEnteredHub();
        else
            flow.SetZone(zone);
    }
}

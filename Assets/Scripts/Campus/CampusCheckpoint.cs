using UnityEngine;

public class CampusCheckpoint : MonoBehaviour
{
    public Transform checkpoint;
    void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<ThirdPersonController>() == null) return;
        var flow = UnityCompat.FindOne<CampusFlowController>();
        if (flow != null && flow.CurrentZone == CampusFlowController.Zone.School) flow.activeCheckpoint = checkpoint;
    }
}

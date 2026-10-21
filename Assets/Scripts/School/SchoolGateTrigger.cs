using UnityEngine;

/// <summary>
/// 校门触发器。玩家进入后通知 SchoolFlowController。
/// </summary>
public class SchoolGateTrigger : MonoBehaviour
{
    public SchoolFlowController flow;

    void Awake()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<SchoolFlowController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (flow == null)
            flow = UnityCompat.FindOne<SchoolFlowController>();
        if (flow != null)
            flow.NotifySchoolEscaped();
    }
}

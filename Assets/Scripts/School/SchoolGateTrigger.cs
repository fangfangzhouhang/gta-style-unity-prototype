using UnityEngine;

/// <summary>
/// 校门触发器。优先 CampusFlow（同场景进公路）；否则 SchoolFlowController。
/// </summary>
public class SchoolGateTrigger : MonoBehaviour
{
    public SchoolFlowController flow;
    public CampusFlowController campusFlow;

    void Awake()
    {
        if (campusFlow == null)
            campusFlow = UnityCompat.FindOne<CampusFlowController>();
        if (flow == null)
            flow = UnityCompat.FindOne<SchoolFlowController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (campusFlow == null)
            campusFlow = UnityCompat.FindOne<CampusFlowController>();
        if (campusFlow != null)
        {
            campusFlow.NotifySchoolEscaped();
            return;
        }
        if (flow == null)
            flow = UnityCompat.FindOne<SchoolFlowController>();
        if (flow != null)
            flow.NotifySchoolEscaped();
    }
}

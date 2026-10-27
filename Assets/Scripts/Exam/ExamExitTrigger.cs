using UnityEngine;

/// <summary>
/// 考场出口触发器。优先通知 CampusFlow（同场景）；否则 ExamFlowController。
/// </summary>
public class ExamExitTrigger : MonoBehaviour
{
    public ExamFlowController flow;
    public CampusFlowController campusFlow;

    void Awake()
    {
        if (campusFlow == null)
            campusFlow = UnityCompat.FindOne<CampusFlowController>();
        if (flow == null)
            flow = UnityCompat.FindOne<ExamFlowController>();
    }

    void OnTriggerStay(Collider other)
    {
        // A locked door may have overlapped this trigger before it was opened.
        if(campusFlow!=null)OnTriggerEnter(other);
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
            campusFlow.NotifyExamEscaped();
            return;
        }
        if (flow == null)
            flow = UnityCompat.FindOne<ExamFlowController>();
        if (flow != null)
            flow.NotifyExamEscaped();
    }
}

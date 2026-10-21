using UnityEngine;

/// <summary>
/// 考场出口触发器。玩家进入后通知 ExamFlowController。
/// </summary>
public class ExamExitTrigger : MonoBehaviour
{
    public ExamFlowController flow;

    void Awake()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<ExamFlowController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (flow == null)
            flow = UnityCompat.FindOne<ExamFlowController>();
        if (flow != null)
            flow.NotifyExamEscaped();
    }
}

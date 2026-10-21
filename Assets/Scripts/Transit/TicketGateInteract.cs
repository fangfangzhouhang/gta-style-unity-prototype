using UnityEngine;

/// <summary>
/// 地铁闸机：有票则通知流程检票通过；无票中文提示。
/// </summary>
public class TicketGateInteract : MonoBehaviour
{
    public TransitFlowController flow;
    public string noTicketHint = "请先到窗口购票";
    public float hintCooldown = 1.4f;

    float nextHintTime;

    void Awake()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<TransitFlowController>();
    }

    void OnTriggerEnter(Collider other)
    {
        TryGate(other);
    }

    void OnTriggerStay(Collider other)
    {
        // 无票时周期性提示；有票由 Flow 一次性处理
        if (TicketInventory.HasTicket)
            return;
        if (other == null || other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (Time.time < nextHintTime)
            return;
        nextHintTime = Time.time + hintCooldown;
        if (flow == null)
            flow = UnityCompat.FindOne<TransitFlowController>();
        if (flow != null)
            flow.NotifyNoTicket(noTicketHint);
    }

    void TryGate(Collider other)
    {
        if (other == null)
            return;
        if (other.GetComponentInParent<ThirdPersonController>() == null)
            return;
        if (flow == null)
            flow = UnityCompat.FindOne<TransitFlowController>();
        if (flow == null)
            return;

        if (TicketInventory.HasTicket)
            flow.NotifyGatePassed();
        else
        {
            nextHintTime = Time.time + hintCooldown;
            flow.NotifyNoTicket(noTicketHint);
        }
    }
}

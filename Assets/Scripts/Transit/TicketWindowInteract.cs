using UnityEngine;

/// <summary>
/// 购票窗口：玩家靠近后按 E 打开中文车次面板。
/// </summary>
public class TicketWindowInteract : MonoBehaviour
{
    public TransitFlowController flow;
    public string promptNear = "按 E 购票（高铁窗口）";
    public float interactRadius = 2.8f;

    Transform player;
    bool inRange;

    public bool PlayerInRange { get { return inRange; } }

    void Start()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<TransitFlowController>();
        ResolvePlayer();
    }

    void ResolvePlayer()
    {
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            player = motor.transform;
    }

    void Update()
    {
        if (flow != null && flow.IsBusy)
            return;

        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
                return;
        }

        float dist = Vector3.Distance(
            new Vector3(player.position.x, 0f, player.position.z),
            new Vector3(transform.position.x, 0f, transform.position.z));
        inRange = dist <= interactRadius;

        if (!inRange)
            return;

        if (flow != null && flow.TicketPanelOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (flow == null)
                flow = UnityCompat.FindOne<TransitFlowController>();
            if (flow != null)
                flow.OpenTicketPanel(this);
        }
    }
}

using UnityEngine;

/// <summary>
/// 城镇赚钱任务点：走进黄圈获得旅游资金（一次性）。
/// </summary>
public class EarnMoneyPoint : MonoBehaviour
{
    public string title = "跑腿任务";
    public int reward = 40;
    public float radius = 2.2f;
    public HubTownFlowController flow;

    public bool IsComplete { get; private set; }

    Transform player;
    bool inside;

    void Start()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<HubTownFlowController>();
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
        if (IsComplete)
            return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;
        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
                return;
        }

        Vector3 a = player.position;
        Vector3 b = transform.position;
        a.y = 0f;
        b.y = 0f;
        bool now = Vector3.Distance(a, b) <= radius;
        if (now && !inside)
        {
            inside = true;
            Complete();
        }
        else if (!now)
            inside = false;
    }

    void Complete()
    {
        if (IsComplete)
            return;
        IsComplete = true;
        Wallet.Add(reward);
        SaveSystem.Save();
        if (flow == null)
            flow = UnityCompat.FindOne<HubTownFlowController>();
        if (flow != null)
            flow.NotifyEarn(title, reward);

        DimVisual();
    }

    void DimVisual()
    {
        Transform parent = transform.parent;
        if (parent == null)
            return;
        Transform visual = parent.Find(gameObject.name + "_Visual");
        if (visual == null)
            return;
        PrimitiveTint tint = visual.GetComponent<PrimitiveTint>();
        if (tint == null)
            return;
        tint.color = new Color(0.35f, 0.35f, 0.35f, 1f);
        Shader shader = PrimitiveTint.FindLitShader();
        if (shader != null)
            tint.Apply(shader);
    }
}

using UnityEngine;

/// <summary>地图边界和坠落兜底，不覆盖正常楼梯和庭院跳跃。</summary>
public class CampusSafety : MonoBehaviour
{
    CampusFlowController flow;
    float nextRecovery;
    void Start() { flow = UnityCompat.FindOne<CampusFlowController>(); }
    void LateUpdate()
    {
        if (flow == null || flow.player == null || Time.time < nextRecovery) return;
        Vector3 p = flow.player.position;
        if (flow.player.GetComponentInParent<CampusMotorcycle>() != null) return;
        if (p.y < -8f || p.x < -108f || p.x > 128f || p.z < -158f || p.z > 238f)
        {
            nextRecovery = Time.time + 1f;
            flow.RecoverFromFall();
        }
    }
}

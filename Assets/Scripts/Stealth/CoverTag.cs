using UnityEngine;

/// <summary>
/// 标记掩体物体，便于识别与调试。实际遮挡靠 Collider + 视野射线。
/// </summary>
public class CoverTag : MonoBehaviour
{
    [Tooltip("可读名称，仅调试。")]
    public string label = "掩体";
}

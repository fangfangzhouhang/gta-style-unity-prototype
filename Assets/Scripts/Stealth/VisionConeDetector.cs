using System;
using UnityEngine;

/// <summary>
/// 监考 / 巡逻用视野锥。玩家进入扇形且射线无遮挡 → 发现。
/// 第一版不做警戒条，直接触发 OnPlayerSpotted。
/// </summary>
public class VisionConeDetector : MonoBehaviour
{
    [Tooltip("视野最远距离（米）。")]
    public float viewDistance = 10f;

    [Tooltip("水平视野全角（度）。")]
    public float viewAngle = 70f;

    [Tooltip("眼睛相对本物体的高度。")]
    public float eyeHeight = 1.55f;

    [Tooltip("检测间隔，秒。")]
    public float checkInterval = 0.12f;

    [Tooltip("被发现后是否继续检测。")]
    public bool keepCheckingAfterSpot = false;

    [Tooltip("可视化扇形（仅 Scene / Gizmo）。")]
    public bool drawGizmos = true;

    public Color gizmoColor = new Color(1f, 0.25f, 0.1f, 0.35f);

    public event Action<VisionConeDetector, Transform> OnPlayerSpotted;

    public bool HasSpotted { get; private set; }

    Transform cachedPlayer;
    float nextCheck;
    void Update()
    {
        if (HasSpotted && !keepCheckingAfterSpot)
            return;

        if (Time.time < nextCheck)
            return;
        nextCheck = Time.time + checkInterval;

        Transform player = ResolvePlayer();
        if (player == null)
            return;

        if (!IsInCone(player.position))
            return;

        if (IsOccluded(player))
            return;

        HasSpotted = true;
        if (OnPlayerSpotted != null)
            OnPlayerSpotted(this, player);
    }

    public void ResetSpot()
    {
        HasSpotted = false;
    }

    Transform ResolvePlayer()
    {
        if (cachedPlayer != null)
            return cachedPlayer;

        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            cachedPlayer = motor.transform;
        return cachedPlayer;
    }

    bool IsInCone(Vector3 worldTarget)
    {
        Vector3 eye = GetEyePosition();
        Vector3 to = worldTarget - eye;
        to.y = 0f;
        float dist = to.magnitude;
        if (dist > viewDistance || dist < 0.05f)
            return false;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        float angle = Vector3.Angle(forward, to / dist);
        return angle <= viewAngle * 0.5f;
    }

    bool IsOccluded(Transform player)
    {
        Vector3 eye = GetEyePosition();
        Vector3 aim = GetAimPoint(player);
        Vector3 delta = aim - eye;
        float dist = delta.magnitude;
        if (dist < 0.05f)
            return false;

        if (!Physics.Raycast(eye, delta / dist, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            return false;

        if (hit.transform == player || hit.transform.IsChildOf(player))
            return false;

        // 撞到掩体或其他障碍 → 遮挡。
        return true;
    }

    Vector3 GetEyePosition()
    {
        return transform.position + Vector3.up * eyeHeight;
    }

    static Vector3 GetAimPoint(Transform player)
    {
        ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
        float y = motor != null && motor.IsCrouching ? 0.55f : 1.0f;
        return player.position + Vector3.up * y;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Vector3 eye = GetEyePosition();
        Gizmos.color = gizmoColor;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        Quaternion left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f);
        Quaternion right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f);
        Vector3 a = eye + left * forward * viewDistance;
        Vector3 b = eye + right * forward * viewDistance;
        Gizmos.DrawLine(eye, a);
        Gizmos.DrawLine(eye, b);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(eye, 0.08f);
    }
}

using UnityEngine;

/// <summary>
/// 被发现后安全回位：与老师/障碍保持距离，短暂无敌，避免连续触发卡死。
/// </summary>
public static class SafeDiscoveryReset
{
    public static float SafeClearance = 3.2f;
    public static float InvulnSeconds = 2.8f;
    public static float MaxPushAttempts = 12;

    static float invulnUntil;
    static Vector3 lastSafePos;

    public static bool IsInvulnerable
    {
        get { return Time.time < invulnUntil; }
    }

    public static void BeginInvulnerability(float seconds = -1f)
    {
        if (seconds < 0f)
            seconds = InvulnSeconds;
        invulnUntil = Time.time + seconds;
    }

    public static bool TryRespawn(Transform player, Transform checkpoint, float clearance = -1f)
    {
        if (player == null)
            return false;
        if (clearance < 0f)
            clearance = SafeClearance;

        Vector3 basePos = checkpoint != null ? checkpoint.position : player.position;
        Quaternion baseRot = checkpoint != null ? checkpoint.rotation : player.rotation;
        Vector3 safe = FindSafePosition(basePos, clearance);

        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = false;

        player.position = safe;
        player.rotation = baseRot;

        if (body != null)
            body.enabled = true;

        ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
        if (motor != null)
        {
            motor.ForceStand();
            motor.ResetVerticalSpeed();
        }

        ThirdPersonCamera cam = UnityCompat.FindOne<ThirdPersonCamera>();
        if (cam != null)
            cam.Snap();

        lastSafePos = safe;
        GameProgress.Ensure().SetCheckpoint(safe);
        BeginInvulnerability();
        ResetAllThreats();
        return true;
    }

    public static Vector3 FindSafePosition(Vector3 preferred, float clearance)
    {
        if (IsClearOfThreats(preferred, clearance) && !IsInsideSolid(preferred))
            return preferred;

        // 径向推开
        for (int i = 1; i <= MaxPushAttempts; i++)
        {
            float radius = clearance * (0.55f + i * 0.35f);
            int spokes = 8;
            for (int s = 0; s < spokes; s++)
            {
                float ang = (s / (float)spokes) * Mathf.PI * 2f + i * 0.2f;
                Vector3 candidate = preferred + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
                candidate.y = preferred.y;
                if (IsClearOfThreats(candidate, clearance * 0.85f) && !IsInsideSolid(candidate))
                    return candidate;
            }
        }

        // 最后手段：沿 +Z 再推远
        Vector3 fallback = preferred + Vector3.forward * (clearance * 2.5f);
        fallback.y = preferred.y;
        return fallback;
    }

    public static bool IsClearOfThreats(Vector3 pos, float clearance)
    {
        TeacherAgent[] agents = UnityCompat.FindAll<TeacherAgent>();
        float clr2 = clearance * clearance;
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] == null)
                continue;
            Vector3 d = agents[i].transform.position - pos;
            d.y = 0f;
            if (d.sqrMagnitude < clr2)
                return false;
        }
        return true;
    }

    static bool IsInsideSolid(Vector3 pos)
    {
        // 脚点略抬高做胶囊重叠检测
        Vector3 center = pos + Vector3.up * 1.0f;
        return Physics.CheckCapsule(pos + Vector3.up * 0.2f, pos + Vector3.up * 1.6f, 0.35f, ~0, QueryTriggerInteraction.Ignore);
    }

    public static void ResetAllThreats()
    {
        TeacherAgent[] agents = UnityCompat.FindAll<TeacherAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] != null)
                agents[i].ResetToPatrol();
        }
        VisionConeDetector[] cones = UnityCompat.FindAll<VisionConeDetector>();
        for (int i = 0; i < cones.Length; i++)
        {
            if (cones[i] != null)
                cones[i].ResetSpot();
        }
        ScrutinyAura[] auras = UnityCompat.FindAll<ScrutinyAura>();
        for (int i = 0; i < auras.Length; i++)
        {
            if (auras[i] != null)
                auras[i].SetForcePulse(false);
        }
    }

    public static void TickStuckWatchdog(Transform player, float maxStillSeconds = 0.35f)
    {
        // 预留：若需要位移检测可扩展；当前以无敌帧为主
        if (player == null)
            return;
        if ((player.position - lastSafePos).sqrMagnitude > 0.01f)
            lastSafePos = player.position;
    }
}

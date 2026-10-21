using UnityEngine;

/// <summary>
/// 审查氛围光：老师用冷红/琥珀点光，家长用冷紫；发现/追赶时脉冲。
/// </summary>
public class ScrutinyAura : MonoBehaviour
{
    public enum Role
    {
        Teacher,
        Parent,
        Patrol
    }

    public Role role = Role.Teacher;
    public float baseIntensity = 1.35f;
    public float pulseIntensity = 2.4f;
    public float range = 7.5f;
    public float pulseSpeed = 3.2f;
    public float height = 1.7f;

    Light point;
    TeacherAgent agent;
    VisionConeDetector cone;
    Color baseColor;
    bool forcePulse;

    public static Color TeacherColor = new Color(1f, 0.28f, 0.12f, 1f);   // 冷红警示
    public static Color TeacherAmber = new Color(1f, 0.55f, 0.12f, 1f);  // 琥珀
    public static Color ParentColor = new Color(0.55f, 0.25f, 1f, 1f);   // 冷紫
    public static Color PatrolColor = new Color(1f, 0.35f, 0.2f, 1f);

    void Awake()
    {
        agent = GetComponent<TeacherAgent>();
        cone = GetComponent<VisionConeDetector>();
        EnsureLight();
        ApplyRoleColor();
    }

    void EnsureLight()
    {
        if (point != null)
            return;
        Transform existing = transform.Find("ScrutinyLight");
        GameObject go;
        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject("ScrutinyLight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, height, 0f);
        }
        point = go.GetComponent<Light>();
        if (point == null)
            point = go.AddComponent<Light>();
        point.type = LightType.Point;
        point.range = range;
        point.intensity = baseIntensity;
        point.shadows = LightShadows.None;
        point.renderMode = LightRenderMode.ForcePixel;
    }

    void ApplyRoleColor()
    {
        switch (role)
        {
            case Role.Parent:
                baseColor = ParentColor;
                break;
            case Role.Patrol:
                baseColor = PatrolColor;
                break;
            default:
                baseColor = Color.Lerp(TeacherColor, TeacherAmber, 0.35f);
                break;
        }
        if (point != null)
            point.color = baseColor;
    }

    public void SetRole(Role r)
    {
        role = r;
        ApplyRoleColor();
    }

    public void SetForcePulse(bool on)
    {
        forcePulse = on;
    }

    void Update()
    {
        if (point == null)
            return;

        bool alert = forcePulse;
        if (!alert && agent != null && agent.CurrentState == TeacherAgent.State.Chase)
            alert = true;
        if (!alert && cone != null && cone.HasSpotted)
            alert = true;

        if (alert)
        {
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
            point.intensity = Mathf.Lerp(baseIntensity, pulseIntensity, wave);
            point.color = Color.Lerp(baseColor, Color.white, wave * 0.25f);
            point.range = range * (1f + wave * 0.12f);
        }
        else
        {
            point.intensity = Mathf.Lerp(point.intensity, baseIntensity, 1f - Mathf.Exp(-6f * Time.deltaTime));
            point.color = Color.Lerp(point.color, baseColor, 1f - Mathf.Exp(-6f * Time.deltaTime));
            point.range = range;
        }
    }
}

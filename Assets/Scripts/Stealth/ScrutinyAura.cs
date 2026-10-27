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
    public float baseIntensity = 1.2f;
    public float pulseIntensity = 2.2f;
    public float range = 6f;
    public float pulseSpeed = 3.2f;
    public float height = 1.7f;

    Light point;
    TeacherAgent agent;
    VisionConeDetector cone;
    Color baseColor;
    bool forcePulse;
    public bool showActorMarker = true;
    static Texture2D bulbIcon;
    Camera markerCamera;
    WorldMapUI markerMap;

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
        point.type = LightType.Spot;
        point.spotAngle = 42f;
        point.innerSpotAngle = 24f;
        go.transform.localPosition = new Vector3(0f, 1.45f, 0.25f);
        go.transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
        point.range = range;
        point.intensity = baseIntensity;
        point.shadows = LightShadows.Soft;
        point.shadowBias = 0.025f;
        point.shadowNormalBias = 0.12f;
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
        EnsureLight();
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

    void OnGUI()
    {
        if(!showActorMarker || !Application.isPlaying || Event.current.type!=EventType.Repaint) return;
        if(PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen) return;
        if(markerMap==null) markerMap=UnityCompat.FindOne<WorldMapUI>();
        if(markerMap!=null && markerMap.IsOpen) return;
        if(markerCamera==null) markerCamera=Camera.main;
        if(markerCamera==null || Vector3.Distance(markerCamera.transform.position,transform.position)>24f) return;
        Vector3 center=transform.position+Vector3.up*2.04f;
        Vector3 screen=markerCamera.WorldToScreenPoint(center);
        if(screen.z<=0 || screen.x<0 || screen.x>Screen.width || screen.y<70 || screen.y>Screen.height-75) return;
        Vector3 chest=transform.position+Vector3.up*1.45f;
        Vector3 delta=chest-markerCamera.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(markerCamera.transform.position,delta.normalized,out hit,delta.magnitude,~0,QueryTriggerInteraction.Ignore) &&
            hit.transform!=transform && !hit.transform.IsChildOf(transform)) return;
        EnsureBulbIcon();
        float size=Mathf.Clamp(Screen.height/1080f,0.65f,1.2f)*30f;
        Rect rect=new Rect(screen.x-size*0.5f,Screen.height-screen.y-size*0.5f,size,size);
        Color previous=GUI.color;
        GUI.color=new Color(0,0,0,0.8f);
        GUI.DrawTexture(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4),bulbIcon);
        GUI.color=role==Role.Parent ? ParentColor : new Color(1f,0.72f,0.18f);
        if(agent!=null && agent.CurrentState==TeacherAgent.State.Chase) GUI.color=new Color(1f,0.2f,0.08f);
        GUI.DrawTexture(rect,bulbIcon);
        GUI.color=previous;
    }

    static void EnsureBulbIcon()
    {
        if(bulbIcon!=null) return;
        const int size=64;
        bulbIcon=new Texture2D(size,size,TextureFormat.RGBA32,false) { name="CatcherBulbMarker", hideFlags=HideFlags.HideAndDontSave };
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++) for(int x=0;x<size;x++)
        {
            Vector2 p=new Vector2((x+0.5f)/size,(y+0.5f)/size);
            bool solid=(p-new Vector2(0.5f,0.58f)).sqrMagnitude<0.19f*0.19f ||
                (p.x>0.41f && p.x<0.59f && p.y>0.22f && p.y<0.49f) ||
                (p.x>0.43f && p.x<0.57f && p.y>0.15f && p.y<0.20f);
            for(int ray=0;ray<7;ray++)
            {
                float angle=ray*Mathf.PI/6f;
                Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Vector2 offset=p-new Vector2(0.5f,0.58f);
                float along=Vector2.Dot(offset,direction);
                float side=Mathf.Abs(offset.x*direction.y-offset.y*direction.x);
                if(along>0.25f && along<0.35f && side<0.018f) solid=true;
            }
            pixels[y*size+x]=solid ? Color.white : Color.clear;
        }
        bulbIcon.SetPixels(pixels); bulbIcon.Apply();
    }
}

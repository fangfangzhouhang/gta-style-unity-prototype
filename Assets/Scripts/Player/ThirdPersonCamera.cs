using UnityEngine;

/// <summary>
/// 鼠标轨道镜头，跟着玩家或车辆。光标锁定后才转镜头，Esc 解锁。
/// 用球体投射把镜头从墙里拉出来。
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 0.7f, 0f);
    public float distance = 5.6f;
    public float lookSensitivity = 2.6f;
    public float minPitch = 8f;
    public float maxPitch = 62f;
    public float followSharpness = 14f;
    public float collisionRadius = 0.28f;
    public float baseFov = 62f;
    public float maxFov = 74f;
    [Tooltip("步行使用第一人称眼高；驾驶仍跟随车辆。")]
    public bool firstPersonOnFoot = true;
    public float firstPersonFov = 68f;
    public float eyeInset = 0.15f;

    Transform hiddenBody;
    Renderer[] bodyRenderers;
    UnityEngine.Rendering.ShadowCastingMode[] bodyShadows;
    public bool IsFirstPerson { get { return firstPersonOnFoot && target != null && target.GetComponent<ThirdPersonController>() != null; } }

    float yaw;
    float pitch = 18f;
    bool hasPose;
    bool aimInitialized;

    public static readonly Vector3 OnFootOffset = new Vector3(0f, 0.7f, 0f);
    public const float OnFootDistance = 5.6f;
    public static readonly Vector3 DrivingOffset = new Vector3(0f, 1.35f, 0f);
    public const float DrivingDistance = 8.4f;

    void Awake()
    {
        if (target == null)
        {
            ThirdPersonController player = UnityCompat.FindOne<ThirdPersonController>();
            if (player != null)
                target = player.transform;
        }
    }

    void Start()
    {
        AimTowardMission();
        Snap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryCapturePointerOnClick();

        // Esc 由 PauseMenu 接管；无暂停菜单时仍解锁鼠标
        if (Input.GetKeyDown(KeyCode.Escape) && PauseMenu.Instance == null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, IsFirstPerson ? -85f : minPitch, IsFirstPerson ? 85f : maxPitch);
    }

    // Shared by real mouse input and the regression check. Modal UI owns its clicks.
    public bool TryCapturePointerOnClick()
    {
        if (CampusJournal.Opened) return false;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen) return false;
        var chapter = UnityCompat.FindOne<CampusExamChapter>();
        if (chapter != null && chapter.KeypadOpen) return false;
        var map = UnityCompat.FindOne<WorldMapUI>();
        if (map != null && map.IsOpen) return false;
        var transit = UnityCompat.FindOne<TransitFlowController>();
        if (transit != null && transit.TicketPanelOpen) return false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        return true;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (!aimInitialized)
            AimTowardMission();

        SyncBodyVisibility();
        if (IsFirstPerson)
        {
            ApplyFirstPersonPose();
            return;
        }

        if (!hasPose)
        {
            Snap();
            return;
        }

        Vector3 pivot = target.position + targetOffset;
        float usedDistance = ResolveDistance(pivot, out Quaternion orbit);
        Vector3 desired = pivot + orbit * new Vector3(0f, 0f, -usedDistance);
        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, t);
        Quaternion look = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, t);
        UpdateFov();
    }

    /// <summary>上下车时切换跟随目标。snap 为 true 时下一帧直接到位，避免镜头穿进车身。</summary>
    public void SetFollow(Transform newTarget, Vector3 offset, float newDistance, bool snap)
    {
        target = newTarget;
        targetOffset = offset;
        distance = newDistance;
        if (snap)
            hasPose = false;
    }

    public void RestoreLook(float heading){yaw=heading;pitch=0;aimInitialized=true;Snap();}

    public void Snap()
    {
        if (target == null)
            return;

        if (!aimInitialized) AimTowardMission();
        SyncBodyVisibility();
        if (IsFirstPerson)
        {
            ApplyFirstPersonPose();
            hasPose = true;
            return;
        }

        Vector3 pivot = target.position + targetOffset;
        float usedDistance = ResolveDistance(pivot, out Quaternion orbit);
        transform.position = pivot + orbit * new Vector3(0f, 0f, -usedDistance);
        Vector3 lookDir = pivot - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        hasPose = true;
        UpdateFov();
    }

    void AimTowardMission()
    {
        if (target == null)
            return;

        Vector3 look = target.forward;
        MissionCheckpoint mission = MissionCheckpoint.Active != null
            ? MissionCheckpoint.Active
            : UnityCompat.FindOne<MissionCheckpoint>();
        if (mission != null)
            look = mission.BeaconWorldPosition - target.position;

        look.y = 0f;
        if (look.sqrMagnitude < 0.001f)
            look = Vector3.forward;

        yaw = Quaternion.LookRotation(look.normalized, Vector3.up).eulerAngles.y;
        pitch = IsFirstPerson ? 0f : 18f;
        aimInitialized = true;
    }

    void ApplyFirstPersonPose()
    {
        var body = target.GetComponent<CharacterController>();
        Vector3 eye = body != null
            ? target.TransformPoint(body.center) + Vector3.up * (body.height * 0.5f - eyeInset)
            : target.position + Vector3.up * 0.65f;
        transform.SetPositionAndRotation(eye, Quaternion.Euler(pitch, yaw, 0f));
        var cam = GetComponent<Camera>();
        if (cam != null) { cam.nearClipPlane = 0.04f; cam.fieldOfView = firstPersonFov; }
        hasPose = true;
    }

    void SyncBodyVisibility()
    {
        Transform wanted = IsFirstPerson ? target : null;
        if (wanted == hiddenBody) return;
        RestoreBody();
        hiddenBody = wanted;
        if (hiddenBody == null) return;
        bodyRenderers = hiddenBody.GetComponentsInChildren<Renderer>();
        bodyShadows = new UnityEngine.Rendering.ShadowCastingMode[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            bodyShadows[i] = bodyRenderers[i].shadowCastingMode;
            bodyRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    void RestoreBody()
    {
        if (bodyRenderers != null)
            for (int i = 0; i < bodyRenderers.Length; i++)
                if (bodyRenderers[i] != null) bodyRenderers[i].shadowCastingMode = bodyShadows[i];
        hiddenBody = null;
        bodyRenderers = null;
        bodyShadows = null;
    }

    void OnDisable() { RestoreBody(); }

    float ResolveDistance(Vector3 pivot, out Quaternion orbit)
    {
        orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = orbit * Vector3.back;
        float used = distance;

        if (Physics.SphereCast(
                pivot,
                collisionRadius,
                direction,
                out RaycastHit hit,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore))
        {
            if (!IsIgnored(hit.transform))
                used = Mathf.Clamp(hit.distance - 0.12f, 0.85f, distance);
        }

        return used;
    }

    bool IsIgnored(Transform hit)
    {
        if (hit == null || target == null)
            return false;
        if (hit == target || hit.IsChildOf(target))
            return true;

        // 人在车里时，镜头目标是车，人也要忽略。
        ThirdPersonController player = UnityCompat.FindOne<ThirdPersonController>();
        if (player != null && (hit == player.transform || hit.IsChildOf(player.transform)))
            return true;
        return false;
    }

    void UpdateFov()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
            return;

        float speed01 = 0f;
        VehicleController vehicle = target != null ? target.GetComponent<VehicleController>() : null;
        if (vehicle != null && vehicle.controlEnabled)
            speed01 = Mathf.Clamp01(vehicle.PlanarSpeed / Mathf.Max(0.01f, vehicle.maxSpeed));

        cam.fieldOfView = Mathf.Lerp(baseFov, maxFov, speed01);
    }
}

using UnityEngine;

/// <summary>
/// 第三人称步行。WASD 相对当前镜头方向移动，鼠标只负责镜头（见 ThirdPersonCamera）。
/// 上车后 PlayerVehicleSwitch 会把 controlEnabled 设为 false，并关掉 CharacterController。
/// 潜行关支持 LeftControl / C 蹲走。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("用来取前进方向的镜头。为空时会用 Main Camera。")]
    public Transform view;

    [Tooltip("为 false 时不响应移动（驾驶中）。")]
    public bool controlEnabled = true;

    [Tooltip("步行速度，米/秒。")]
    public float walkSpeed = 4.4f;

    [Tooltip("按住 Shift 时的速度，米/秒。")]
    public float sprintSpeed = 7.4f;

    [Tooltip("蹲走速度，米/秒。")]
    public float crouchSpeed = 2.2f;

    [Tooltip("站立时 CharacterController 高度。")]
    public float standingHeight = 2f;

    [Tooltip("蹲下时 CharacterController 高度。")]
    public float crouchHeight = 1.15f;

    [Tooltip("为 false 时禁用蹲（开世界切片可关掉）。")]
    public bool allowCrouch = true;

    [Tooltip("转向平滑，越大越跟手。")]
    public float turnSharpness = 14f;

    public float gravity = -28f;
    public float jumpSpeed = 7.2f;

    CharacterController body;
    float verticalSpeed;
    float standingCenterY;
    float crouchCenterY;

    public bool IsCrouching { get; private set; }

    void Awake()
    {
        body = GetComponent<CharacterController>();
        if (body != null)
        {
            if (standingHeight <= 0.1f)
                standingHeight = body.height;
            standingCenterY = body.center.y;
            crouchCenterY = crouchHeight * 0.5f - 0.05f;
        }
    }

    void Update()
    {
        if (!controlEnabled || body == null || !body.enabled)
            return;

        if (view == null && Camera.main != null)
            view = Camera.main.transform;

        UpdateCrouch();

        Vector3 move = ReadMoveDirection();
        bool sprint = !IsCrouching && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        float speed = IsCrouching ? crouchSpeed : (sprint ? sprintSpeed : walkSpeed);

        // isGrounded 在刚落地的几帧会闪，贴地时把下坠速度夹住，避免越落越快。
        if (body.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;

        if (body.isGrounded && !IsCrouching && Input.GetKeyDown(KeyCode.Space))
            verticalSpeed = jumpSpeed;

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalSpeed;
        body.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(move, Vector3.up);
            float t = 1f - Mathf.Exp(-turnSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, face, t);
        }
    }

    /// <summary>下车后清掉下落速度，避免继承驾驶时的垂直速度。</summary>
    public void ResetVerticalSpeed()
    {
        verticalSpeed = -2f;
    }

    /// <summary>传送后强制站立，避免蹲姿高度残留。</summary>
    public void ForceStand()
    {
        IsCrouching = false;
        ApplyCrouchPose();
    }

    void UpdateCrouch()
    {
        if (!allowCrouch)
        {
            if (IsCrouching)
            {
                IsCrouching = false;
                ApplyCrouchPose();
            }
            return;
        }

        bool wantCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        if (wantCrouch == IsCrouching)
            return;

        if (!wantCrouch && !CanStandUp())
            return;

        IsCrouching = wantCrouch;
        ApplyCrouchPose();
    }

    bool CanStandUp()
    {
        if (body == null)
            return true;

        float radius = Mathf.Max(0.05f, body.radius * 0.95f);
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);
        float cast = Mathf.Max(0.1f, standingHeight - radius * 2f - 0.1f);
        return !Physics.SphereCast(
            origin,
            radius,
            Vector3.up,
            out _,
            cast,
            ~0,
            QueryTriggerInteraction.Ignore);
    }

    void ApplyCrouchPose()
    {
        if (body == null)
            return;

        body.height = IsCrouching ? crouchHeight : standingHeight;
        Vector3 center = body.center;
        center.y = IsCrouching ? crouchCenterY : standingCenterY;
        body.center = center;

        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            float sy = IsCrouching ? 0.58f : 0.92f;
            Vector3 scale = visual.localScale;
            scale.y = sy;
            visual.localScale = scale;
            visual.localPosition = IsCrouching ? new Vector3(0f, -0.2f, 0f) : Vector3.zero;
        }
    }

    Vector3 ReadMoveDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (view != null)
        {
            forward = view.forward;
            right = view.right;
        }

        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.right;
        else
            right.Normalize();

        Vector3 move = forward * v + right * h;
        if (move.sqrMagnitude > 1f)
            move.Normalize();
        return move;
    }
}

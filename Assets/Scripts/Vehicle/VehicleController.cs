using UnityEngine;

/// <summary>
/// 街机手感的车：W/S 加减速，A/D 转向，空格刹车。
/// 冻结 X/Z 旋转，避免方块车被撞翻。横向抓地用加速度抵消侧滑。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Tooltip("只有上车后才为 true。")]
    public bool controlEnabled;

    public float engineAccel = 16f;
    public float reverseAccel = 10f;
    public float brakeAccel = 28f;
    public float coastAccel = 3.2f;
    public float maxSpeed = 18f;
    public float maxReverseSpeed = 6.5f;
    public float lateralGrip = 12f;
    public float steerDegrees = 115f;
    public float wheelRadius = 0.34f;

    [Tooltip("前轮父节点，只负责视觉转向。")]
    public Transform[] steerHubs;

    [Tooltip("轮胎网格，绕自身 Y 轴滚动（网格已绕 Z 转了 90 度，本地 Y 是车轴）。")]
    public Transform[] spinWheels;

    Rigidbody body;
    float steerVisual;

    public float PlanarSpeed
    {
        get
        {
            if (body == null)
                return 0f;
            Vector3 v = body.velocity;
            v.y = 0f;
            return v.magnitude;
        }
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.mass = 900f;
        body.drag = 0.04f;
        body.angularDrag = 2.5f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.centerOfMass = new Vector3(0f, 0.3f, 0.1f);
        ResolveWheelRefs();
    }

    void ResolveWheelRefs()
    {
        if (steerHubs == null || steerHubs.Length == 0)
        {
            steerHubs = new[]
            {
                transform.Find("WheelFL"),
                transform.Find("WheelFR")
            };
        }

        if (spinWheels == null || spinWheels.Length == 0)
        {
            spinWheels = new[]
            {
                transform.Find("WheelFL/WheelVisual"),
                transform.Find("WheelFR/WheelVisual"),
                transform.Find("WheelRL/WheelVisual"),
                transform.Find("WheelRR/WheelVisual")
            };
        }
    }

    void FixedUpdate()
    {
        if (body == null)
            return;

        Vector3 forward = FlatForward();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        float forwardSpeed = Vector3.Dot(body.velocity, forward);

        if (!controlEnabled)
        {
            // 没人开时自己慢慢停住，避免车自己滑走。
            Vector3 parked = body.velocity;
            Vector3 flat = Vector3.Lerp(new Vector3(parked.x, 0f, parked.z), Vector3.zero, Time.fixedDeltaTime * 2.2f);
            body.velocity = new Vector3(flat.x, parked.y, flat.z);
            body.angularVelocity = new Vector3(0f, body.angularVelocity.y * 0.9f, 0f);
            UpdateWheelVisuals(forwardSpeed, 0f);
            return;
        }

        body.WakeUp();
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        bool braking = Input.GetKey(KeyCode.Space);

        if (braking)
        {
            Vector3 v = body.velocity;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);
            horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, brakeAccel * Time.fixedDeltaTime);
            body.velocity = new Vector3(horizontal.x, v.y, horizontal.z);
        }
        else if (throttle > 0.01f && forwardSpeed < maxSpeed)
        {
            body.AddForce(forward * (throttle * engineAccel), ForceMode.Acceleration);
        }
        else if (throttle < -0.01f && forwardSpeed > -maxReverseSpeed)
        {
            body.AddForce(forward * (throttle * reverseAccel), ForceMode.Acceleration);
        }
        else if (Mathf.Abs(throttle) <= 0.01f)
        {
            // 松油门只沿车头方向减速，侧滑交给下面的抓地。
            Vector3 v = body.velocity;
            Vector3 fwdVel = forward * Vector3.Dot(v, forward);
            Vector3 lateral = v - new Vector3(0f, v.y, 0f) - fwdVel;
            fwdVel = Vector3.MoveTowards(fwdVel, Vector3.zero, coastAccel * Time.fixedDeltaTime);
            body.velocity = new Vector3(fwdVel.x + lateral.x, v.y, fwdVel.z + lateral.z);
        }

        // 速度上限只削掉沿车头方向的分量，碰撞产生的侧向速度留给抓地去收。
        forwardSpeed = Vector3.Dot(body.velocity, forward);
        if (forwardSpeed > maxSpeed)
            body.velocity -= forward * (forwardSpeed - maxSpeed);
        else if (forwardSpeed < -maxReverseSpeed)
            body.velocity -= forward * (forwardSpeed + maxReverseSpeed);

        float lateral = Vector3.Dot(body.velocity, right);
        float grip = braking ? lateralGrip * 0.25f : lateralGrip;
        body.AddForce(-right * lateral * grip, ForceMode.Acceleration);

        bool allowSteer = Mathf.Abs(forwardSpeed) > 0.35f || Mathf.Abs(throttle) > 0.05f;
        if (allowSteer && Mathf.Abs(steer) > 0.01f)
        {
            float speed01 = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
            float rate = Mathf.Lerp(steerDegrees, steerDegrees * 0.42f, speed01);
            float steerSign = forwardSpeed < -0.4f ? -1f : 1f;
            float yaw = steer * rate * steerSign * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw, 0f));
        }

        body.angularVelocity = new Vector3(0f, body.angularVelocity.y, 0f);
        UpdateWheelVisuals(forwardSpeed, steer);
    }

    void UpdateWheelVisuals(float forwardSpeed, float steerInput)
    {
        float targetSteer = controlEnabled ? -steerInput * 32f : 0f;
        steerVisual = Mathf.Lerp(steerVisual, targetSteer, 1f - Mathf.Exp(-12f * Time.fixedDeltaTime));

        if (steerHubs != null)
        {
            for (int i = 0; i < steerHubs.Length; i++)
            {
                if (steerHubs[i] != null)
                    steerHubs[i].localRotation = Quaternion.Euler(0f, steerVisual, 0f);
            }
        }

        if (spinWheels == null || wheelRadius <= 0.01f)
            return;

        // 网格绕 Z 转 90° 后，本地 +Y 指向世界 -X，所以前进时用负角度才是往前滚。
        float spin = -forwardSpeed / wheelRadius * Mathf.Rad2Deg * Time.fixedDeltaTime;
        for (int i = 0; i < spinWheels.Length; i++)
        {
            if (spinWheels[i] != null)
                spinWheels[i].Rotate(0f, spin, 0f, Space.Self);
        }
    }

    Vector3 FlatForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            return Vector3.forward;
        return forward.normalized;
    }
}

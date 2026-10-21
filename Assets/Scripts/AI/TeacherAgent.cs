using System;
using UnityEngine;

/// <summary>
/// 老师 AI：随机路径点巡逻 → 视野发现后追赶 → 靠近抓住。
/// 与 VisionConeDetector 配合；抓住后由 Exam/School Flow 送回座位学习。
/// </summary>
public class TeacherAgent : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Hold
    }

    public Transform[] waypoints;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.6f;
    public float catchDistance = 1.4f;
    public float arriveDistance = 0.4f;
    public float waitAtPoint = 0.55f;
    public bool randomPatrol = true;
    public float loseSightReturnSeconds = 4.5f;

    public event Action<TeacherAgent, Transform> OnCaughtPlayer;

    public State CurrentState { get; private set; }

    VisionConeDetector cone;
    Transform player;
    int wpIndex;
    float waitRemaining;
    float loseSightTimer;
    bool paused;

    void Awake()
    {
        cone = GetComponent<VisionConeDetector>();
        if (cone != null)
        {
            cone.keepCheckingAfterSpot = true;
            cone.OnPlayerSpotted += HandleSpotted;
        }
        CurrentState = State.Patrol;
        if (waypoints != null && waypoints.Length > 0)
            wpIndex = UnityEngine.Random.Range(0, waypoints.Length);
    }

    void OnDestroy()
    {
        if (cone != null)
            cone.OnPlayerSpotted -= HandleSpotted;
    }

    void HandleSpotted(VisionConeDetector source, Transform spotted)
    {
        if (paused || CurrentState == State.Hold)
            return;
        player = spotted;
        if (CurrentState != State.Chase)
            CurrentState = State.Chase;
        loseSightTimer = loseSightReturnSeconds;
    }

    public void SetPaused(bool value)
    {
        paused = value;
    }

    public void ResetToPatrol()
    {
        CurrentState = State.Patrol;
        waitRemaining = 0.3f;
        loseSightTimer = 0f;
        if (cone != null)
            cone.ResetSpot();
    }

    void Update()
    {
        if (paused)
            return;

        if (player == null)
        {
            ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
            if (motor != null)
                player = motor.transform;
        }

        switch (CurrentState)
        {
            case State.Patrol:
                TickPatrol();
                break;
            case State.Chase:
                TickChase();
                break;
            case State.Hold:
                break;
        }
    }

    void TickPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (waitRemaining > 0f)
        {
            waitRemaining -= Time.deltaTime;
            return;
        }

        Transform target = waypoints[Mathf.Clamp(wpIndex, 0, waypoints.Length - 1)];
        if (target == null)
        {
            PickNextWaypoint();
            return;
        }

        if (MoveToward(target.position, patrolSpeed))
        {
            waitRemaining = waitAtPoint;
            PickNextWaypoint();
        }
    }

    void TickChase()
    {
        if (player == null)
        {
            CurrentState = State.Patrol;
            return;
        }

        // 若仍在锥内则刷新追击计时
        if (cone != null && cone.HasSpotted)
            loseSightTimer = loseSightReturnSeconds;
        else
        {
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                CurrentState = State.Patrol;
                if (cone != null)
                    cone.ResetSpot();
                return;
            }
        }

        Vector3 goal = player.position;
        MoveToward(goal, chaseSpeed);

        Vector3 flat = player.position - transform.position;
        flat.y = 0f;
        if (flat.magnitude <= catchDistance)
            CatchPlayer();
    }

    void CatchPlayer()
    {
        CurrentState = State.Hold;
        if (OnCaughtPlayer != null)
            OnCaughtPlayer(this, player);
    }

    bool MoveToward(Vector3 worldGoal, float speed)
    {
        Vector3 pos = transform.position;
        Vector3 goal = worldGoal;
        goal.y = pos.y;
        Vector3 delta = goal - pos;
        float dist = delta.magnitude;
        if (dist <= arriveDistance)
            return true;

        Vector3 step = delta.normalized * (speed * Time.deltaTime);
        if (step.magnitude > dist)
            step = delta;
        transform.position = pos + step;

        if (delta.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(delta.normalized, Vector3.up),
                1f - Mathf.Exp(-12f * Time.deltaTime));
        }
        return false;
    }

    void PickNextWaypoint()
    {
        if (waypoints == null || waypoints.Length <= 1)
            return;

        if (randomPatrol)
        {
            int next = wpIndex;
            int guard = 0;
            while (next == wpIndex && guard++ < 12)
                next = UnityEngine.Random.Range(0, waypoints.Length);
            wpIndex = next;
        }
        else
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
        }
    }
}

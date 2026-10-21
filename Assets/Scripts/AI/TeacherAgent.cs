using System;
using UnityEngine;
using UnityEngine.AI;

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
    NavMeshAgent navigation;
    Vector3 homePosition, lastKnownPosition;
    Quaternion homeRotation;
    float immunityUntil, repathAt;

    void Start()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        if (navigation == null) navigation = gameObject.AddComponent<NavMeshAgent>();
        navigation.radius = 0.27f;
        navigation.height = 1.75f;
        navigation.acceleration = 7f;
        navigation.angularSpeed = 180f;
        navigation.stoppingDistance = 0.2f;
        navigation.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(homePosition, out hit, 0.8f, NavMesh.AllAreas))
        { navigation.enabled = true; navigation.Warp(hit.position); homePosition = hit.position; }
        else navigation.enabled = false;
    }

    public void ReturnHome()
    {
        if (navigation != null && navigation.enabled && navigation.isOnNavMesh) navigation.Warp(homePosition);
        else transform.position = homePosition;
        transform.rotation = homeRotation;
        ResetToPatrol();
    }

    void Awake()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        immunityUntil = Time.time + 3.5f;
        navigation = GetComponent<NavMeshAgent>();
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
        if (paused || CurrentState == State.Hold || Time.time < immunityUntil || SafeDiscoveryReset.IsInvulnerable)
            return;
        player = spotted;
        lastKnownPosition = spotted.position;
        if (CurrentState != State.Chase)
            CurrentState = State.Chase;
        loseSightTimer = loseSightReturnSeconds;
    }

    public void SetPaused(bool value)
    {
        paused = value;
        if (navigation != null && navigation.enabled && navigation.isOnNavMesh) navigation.isStopped = value;
    }

    public void ResetToPatrol()
    {
        CurrentState = State.Patrol;
        waitRemaining = 0.7f;
        immunityUntil = Time.time + 3.5f;
        if (navigation != null && navigation.enabled && navigation.isOnNavMesh) navigation.ResetPath();
        loseSightTimer = 0f;
        if (cone != null)
            cone.ResetSpot();
    }

    void Update()
    {
        if (paused || (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen))
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
        if (player == null || SafeDiscoveryReset.IsInvulnerable) { ResetToPatrol(); return; }
        bool visible = cone != null && cone.CanSeePlayer(player);
        if (visible) { lastKnownPosition = player.position; loseSightTimer = loseSightReturnSeconds; }
        else loseSightTimer -= Time.deltaTime;
        if (loseSightTimer <= 0f || Vector3.Distance(transform.position, player.position) > 15f)
        { ResetToPatrol(); return; }
        MoveToward(lastKnownPosition, chaseSpeed);
        Vector3 flat = player.position - transform.position;
        flat.y = 0f;
        if (visible && flat.magnitude <= catchDistance &&
            Mathf.Abs(player.position.y - (transform.position.y + 0.9f)) < 0.85f && Time.time >= immunityUntil)
            CatchPlayer();
    }

    void CatchPlayer()
    {
        CurrentState = State.Hold;
        if (navigation != null && navigation.enabled && navigation.isOnNavMesh) navigation.ResetPath();
        if (OnCaughtPlayer != null)
            OnCaughtPlayer(this, player);
    }

    bool MoveToward(Vector3 goal, float speed)
    {
        if (navigation != null && navigation.enabled && navigation.isOnNavMesh)
        {
            navigation.speed = speed;
            if (Time.time >= repathAt)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(goal, out hit, 1.2f, navigation.areaMask)) navigation.SetDestination(hit.position);
                repathAt = Time.time + 0.2f;
            }
            Vector3 delta = transform.position - goal;
            delta.y = 0f;
            return !navigation.pathPending && delta.magnitude <= arriveDistance;
        }
        Vector3 direction = goal - transform.position;
        direction.y = 0f;
        if (direction.magnitude <= arriveDistance) return true;
        float length = Mathf.Min(direction.magnitude, speed * Time.deltaTime);
        direction.Normalize();
        foreach (var hit in Physics.CapsuleCastAll(transform.position + Vector3.up * 0.3f,
            transform.position + Vector3.up * 1.45f, 0.26f, direction, length + 0.04f, ~0, QueryTriggerInteraction.Ignore))
            if (!hit.transform.IsChildOf(transform) && hit.transform != transform) return false;
        transform.position += direction * length;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 180f * Time.deltaTime);
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

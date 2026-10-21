using UnityEngine;

/// <summary>
/// 在路点间来回走。学校巡逻与监考轻微踱步都可用。
/// </summary>
public class SimplePatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 2.2f;
    public float arriveDistance = 0.35f;
    public float waitAtPoint = 0.8f;
    public bool pingPong = true;

    int index;
    int direction = 1;
    float waitRemaining;
    bool paused;

    public void SetPaused(bool value)
    {
        paused = value;
    }

    void Update()
    {
        if (paused || waypoints == null || waypoints.Length == 0)
            return;

        if (waitRemaining > 0f)
        {
            waitRemaining -= Time.deltaTime;
            return;
        }

        Transform target = waypoints[Mathf.Clamp(index, 0, waypoints.Length - 1)];
        if (target == null)
        {
            Advance();
            return;
        }

        Vector3 pos = transform.position;
        Vector3 goal = target.position;
        goal.y = pos.y;
        Vector3 delta = goal - pos;
        float dist = delta.magnitude;
        if (dist <= arriveDistance)
        {
            waitRemaining = waitAtPoint;
            Advance();
            return;
        }

        Vector3 step = delta.normalized * (moveSpeed * Time.deltaTime);
        if (step.magnitude > dist)
            step = delta;
        transform.position = pos + step;

        if (delta.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(delta.normalized, Vector3.up),
                1f - Mathf.Exp(-10f * Time.deltaTime));
    }

    void Advance()
    {
        if (waypoints.Length <= 1)
            return;

        if (pingPong)
        {
            if (index + direction >= waypoints.Length || index + direction < 0)
                direction = -direction;
            index += direction;
        }
        else
        {
            index = (index + 1) % waypoints.Length;
        }
    }
}

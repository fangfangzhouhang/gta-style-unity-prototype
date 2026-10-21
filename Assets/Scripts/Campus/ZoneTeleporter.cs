using UnityEngine;

/// <summary>
/// 同场景区域传送（秘密通道终点 / 捷径）。靠近按 E，或设为自动触发。
/// </summary>
public class ZoneTeleporter : MonoBehaviour
{
    public Transform destination;
    public string prompt = "按 E 进入秘密通道";
    public bool autoTrigger;
    public float interactRadius = 2.2f;
    public bool requireCrouch;

    Transform player;
    public bool PlayerInRange { get; private set; }

    void Start()
    {
        ResolvePlayer();
    }

    void ResolvePlayer()
    {
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            player = motor.transform;
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;
        if (destination == null)
            return;
        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
                return;
        }

        float dist = Vector3.Distance(
            new Vector3(player.position.x, 0f, player.position.z),
            new Vector3(transform.position.x, 0f, transform.position.z));
        PlayerInRange = dist <= interactRadius;
        if (!PlayerInRange)
            return;

        if (autoTrigger)
        {
            Teleport();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            Teleport();
    }

    void Teleport()
    {
        if (requireCrouch)
        {
            ThirdPersonController motor = player.GetComponent<ThirdPersonController>();
            if (motor != null && !motor.IsCrouching)
                return;
        }

        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = false;
        player.position = destination.position + Vector3.up * 0.05f;
        player.rotation = destination.rotation;
        if (body != null)
            body.enabled = true;

        SafeDiscoveryReset.BeginInvulnerability(1.2f);
        ThirdPersonCamera cam = UnityCompat.FindOne<ThirdPersonCamera>();
        if (cam != null)
            cam.Snap();

        CampusFlowController campus = UnityCompat.FindOne<CampusFlowController>();
        if (campus != null)
            campus.NotifySecretUsed(prompt);
    }
}

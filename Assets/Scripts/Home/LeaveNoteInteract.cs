using UnityEngine;

/// <summary>
/// 桌上留纸条：成功写下中文留言且未被家长发现。
/// </summary>
public class LeaveNoteInteract : MonoBehaviour
{
    public HomeFlowController flow;
    public string promptNear = "按 E 在桌上留下纸条";
    public float interactRadius = 2.4f;

    public const string NoteText = "爸妈：我要出去旅游，路费是我自己赚的。放心。";

    Transform player;
    public bool PlayerInRange { get; private set; }
    public bool Done { get; private set; }

    void Start()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<HomeFlowController>();
        ResolvePlayer();
        if (GameProgress.Ensure().leftNote)
            Done = true;
    }

    void ResolvePlayer()
    {
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            player = motor.transform;
    }

    void Update()
    {
        if (Done || (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen))
            return;
        if (flow != null && flow.IsBusy)
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            Done = true;
            GameProgress.Ensure().MarkLeftNote();
            SaveSystem.Save();
            if (flow == null)
                flow = UnityCompat.FindOne<HomeFlowController>();
            if (flow != null)
                flow.NotifyNoteLeft(NoteText);
        }
    }
}

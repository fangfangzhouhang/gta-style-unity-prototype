using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景传送门：靠近按 E 进入目标场景（可选门槛检查）。
/// </summary>
public class ScenePortalInteract : MonoBehaviour
{
    public string targetScene;
    public string promptNear = "按 E 进入";
    public float interactRadius = 2.8f;
    public bool requireEscapedSchool;
    public bool requireLeftNote;
    public bool requirePhone;
    public HubTownFlowController flow;

    Transform player;
    public bool PlayerInRange { get; private set; }

    void Start()
    {
        if (flow == null)
            flow = UnityCompat.FindOne<HubTownFlowController>();
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
        if(CampusJournal.Opened)return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;
        var departure=UnityCompat.FindOne<CampusDepartureChapter>();
        if(departure!=null && !departure.State.completed){PlayerInRange=false;return;}
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
            TryEnter();
    }

    void TryEnter()
    {
        var departure=UnityCompat.FindOne<CampusDepartureChapter>();
        if(departure!=null && !departure.State.completed){Tip("先骑车离开校门",departure.Detail);return;}
        GameProgress gp = GameProgress.Ensure();
        if (requireEscapedSchool && !gp.escapedSchool)
        {
            Tip("尚未逃出教学楼", "先完成考场与学校逃离");
            return;
        }
        if (requireLeftNote && !gp.leftNote)
        {
            Tip("还没留纸条", "先回家在桌上留条，且别被家长发现");
            return;
        }
        if (requirePhone && !gp.hasPhone)
        {
            Tip("需要手机", "先到路边摊购买手机才能查看票价/购票");
            return;
        }
        if (string.IsNullOrEmpty(targetScene))
            return;
        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Tip("场景未就绪", "请用 EscapeExam 菜单生成并写入 Build Settings：" + targetScene);
            return;
        }
        SaveSystem.Save();
        SceneManager.LoadScene(targetScene);
    }

    void Tip(string title, string sub)
    {
        if (flow == null)
            flow = UnityCompat.FindOne<HubTownFlowController>();
        if (flow != null)
            flow.ShowTip(title, sub);
    }
}

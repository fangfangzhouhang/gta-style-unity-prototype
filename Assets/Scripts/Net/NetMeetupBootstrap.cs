using UnityEngine;

/// <summary>
/// 独立汇合场景入口（可选；联机考场内已含汇合点）。
/// </summary>
public class NetMeetupBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        EscapeNetHud.Ensure();
        if (GameObject.Find("MeetupRoot") == null)
            NetMeetupBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
        EscapeNetHud.Ensure().SetObjective("汇合点", "等待队友到达");
    }
}

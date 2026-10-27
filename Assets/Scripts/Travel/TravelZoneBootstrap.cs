using UnityEngine;

/// <summary>
/// 旅行任务场景入口。空场景时生成地标任务区。
/// </summary>
public class TravelZoneBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        TravelZoneBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

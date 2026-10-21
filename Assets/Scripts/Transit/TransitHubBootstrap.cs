using UnityEngine;

/// <summary>
/// 交通枢纽场景入口。空场景时生成综合站。
/// </summary>
public class TransitHubBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        TransitHubBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

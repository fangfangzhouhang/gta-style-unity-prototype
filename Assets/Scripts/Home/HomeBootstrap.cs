using UnityEngine;

/// <summary>
/// 家庭场景入口。无 LevelRoot 时生成整屋。
/// </summary>
public class HomeBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        HomeBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

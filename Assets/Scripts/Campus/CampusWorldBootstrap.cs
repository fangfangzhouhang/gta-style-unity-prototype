using UnityEngine;

/// <summary>
/// CampusWorld 入口：无 LevelRoot 时自动生成连通大地图。
/// </summary>
public class CampusWorldBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        CampusWorldBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

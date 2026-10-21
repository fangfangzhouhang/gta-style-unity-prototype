using UnityEngine;

/// <summary>
/// 考场场景入口。若场景里还没有 LevelRoot，则生成整间考场。
/// </summary>
public class ExamHallBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        ExamHallBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

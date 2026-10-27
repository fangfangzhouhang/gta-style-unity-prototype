using UnityEngine;

/// <summary>
/// 学校逃离场景入口。空场景时生成走廊关卡。
/// </summary>
public class SchoolEscapeBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        SchoolEscapeBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

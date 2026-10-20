using UnityEngine;

/// <summary>
/// 场景入口。如果场景里已经摆好了玩家就直接玩；
/// 如果是空场景，则在进入 Play 时生成整座占位街区。
/// </summary>
public class PrototypeBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (GameObject.Find("Player") != null || GameObject.Find("City") != null)
            return;

        PrototypeWorldBuilder.Build();
    }

    void Start()
    {
        // 按当前管线重新上色，URP 下避免 Standard 材质变粉。
        PrimitiveTint.ApplyAll();
    }
}

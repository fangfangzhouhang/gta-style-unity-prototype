using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 运行时拼装 / 加载带 NetworkObject 的玩家模板，供 Host/Client 注册为 PlayerPrefab。
/// 优先 Resources/NetworkPlayer（由编辑器菜单生成）；否则动态创建。
/// 首次打开工程等 Package Manager 解析 NGO 后再 Play。
/// </summary>
public static class NetPlayerTemplateFactory
{
    const string TemplateName = "NetworkPlayerTemplate";

    public static GameObject EnsureTemplate()
    {
        GameObject fromResources = Resources.Load<GameObject>("NetworkPlayer");
        if (fromResources != null)
            return fromResources;

        GameObject existing = GameObject.Find(TemplateName);
        if (existing != null)
            return existing;

        var root = new GameObject(TemplateName);
        Object.DontDestroyOnLoad(root);
        root.SetActive(false);

        var body = root.AddComponent<CharacterController>();
        body.height = 2f;
        body.radius = 0.4f;
        body.center = Vector3.zero;
        body.stepOffset = 0.35f;
        body.skinWidth = 0.08f;
        body.minMoveDistance = 0.001f;

        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(0.8f, 0.92f, 0.8f);
        Object.Destroy(visual.GetComponent<Collider>());

        var motor = root.AddComponent<ThirdPersonController>();
        motor.allowCrouch = true;

        root.AddComponent<NetworkObject>();
        root.AddComponent<NetworkPlayer>();
        root.AddComponent<EscapeNetProgress>();

        return root;
    }
}

using UnityEngine;

/// <summary>
/// 考场 / 学校占位关卡共用的几何体与相机灯光工具。
/// 与 PrototypeWorldBuilder 并列，避免改动开世界生成路径。
/// </summary>
public static class LevelBuildUtil
{
    static readonly System.Collections.Generic.Dictionary<PrimitiveType, Mesh> MeshCache =
        new System.Collections.Generic.Dictionary<PrimitiveType, Mesh>();

    public static void EnsureSun()
    {
        Light light = UnityCompat.FindOne<Light>();
        if (light == null)
        {
            var sun = new GameObject("Sun");
            light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.96f, 0.88f);
        light.shadows = LightShadows.Soft;
        RenderSettings.sun = light;
    }

    public static Camera EnsureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        cam.clearFlags = RenderSettings.skybox != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.74f, 0.92f);
        cam.nearClipPlane = 0.15f;
        cam.farClipPlane = 800f;
        cam.fieldOfView = 62f;
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
        if (cam.GetComponent<ThirdPersonCamera>() == null)
            cam.gameObject.AddComponent<ThirdPersonCamera>();
        return cam;
    }

    public static GameObject CreatePlayer(Transform parent, Vector3 position, bool allowCrouch)
    {
        var player = new GameObject("Player");
        player.transform.SetParent(parent, false);
        player.transform.position = position;

        var body = player.AddComponent<CharacterController>();
        body.height = 2f;
        body.radius = 0.4f;
        body.center = Vector3.zero;
        body.stepOffset = 0.35f;
        body.skinWidth = 0.08f;
        body.minMoveDistance = 0.001f;

        Make(PrimitiveType.Capsule, player.transform, "Visual",
            Vector3.zero, new Vector3(0.8f, 0.92f, 0.8f), Quaternion.identity,
            CityPalette.Player, "Player", false);

        var motor = player.AddComponent<ThirdPersonController>();
        motor.allowCrouch = allowCrouch;
        return player;
    }

    public static GameObject Make(
        PrimitiveType type,
        Transform parent,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Quaternion localRot,
        Color color,
        string materialName,
        bool collider,
        bool emission = false,
        float smoothness = 0.18f)
    {
        var go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;

        var filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = BuiltinMesh(type);
        go.AddComponent<MeshRenderer>();

        if (collider)
        {
            switch (type)
            {
                case PrimitiveType.Sphere:
                    go.AddComponent<SphereCollider>();
                    break;
                case PrimitiveType.Capsule:
                    go.AddComponent<CapsuleCollider>();
                    break;
                default:
                    go.AddComponent<BoxCollider>();
                    break;
            }
        }

        var tint = go.AddComponent<PrimitiveTint>();
        tint.color = color;
        tint.materialName = materialName;
        tint.emission = emission;
        tint.smoothness = smoothness;
        AssignEditorMaterial(go, tint);
        return go;
    }

    public static Mesh BuiltinMesh(PrimitiveType type)
    {
        Mesh mesh;
        if (MeshCache.TryGetValue(type, out mesh) && mesh != null)
            return mesh;

        GameObject temp = GameObject.CreatePrimitive(type);
        mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        temp.SetActive(false);
        Object.DestroyImmediate(temp);
        MeshCache[type] = mesh;
        return mesh;
    }

    public static void AssignEditorMaterial(GameObject go, PrimitiveTint tint)
    {
#if UNITY_EDITOR
        if (Application.isPlaying || tint == null || string.IsNullOrEmpty(tint.materialName))
            return;

        Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/" + tint.materialName + ".mat");
        Renderer renderer = go.GetComponent<Renderer>();
        if (mat != null && renderer != null)
            renderer.sharedMaterial = mat;
#endif
    }

    public static void DestroyByName(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
            DestroyObject(go);
    }

    public static void DestroyObject(Object obj)
    {
        if (obj == null)
            return;
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
    public static void BakeCampusMaterials()
    {
#if UNITY_EDITOR
        const string directory = "Assets/Materials/Campus";
        if (!UnityEditor.AssetDatabase.IsValidFolder(directory)) UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "Campus");
        foreach (var tint in UnityEngine.Object.FindObjectsOfType<PrimitiveTint>())
        {
            // 按完整外观建立共享材质，保证编辑器与 Play 配色一致。
            string key = ColorUtility.ToHtmlStringRGBA(tint.color) + "_" +
                Mathf.RoundToInt(tint.smoothness * 1000) + (tint.emission ? "_glow" : "");
            string path = directory + "/Campus_" + key + ".mat";
            Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                tint.Apply(Shader.Find("Standard"));
                material = tint.GetComponent<Renderer>().sharedMaterial;
                UnityEditor.AssetDatabase.CreateAsset(material, path);
            }
            tint.GetComponent<Renderer>().sharedMaterial = material;
        }
#endif
    }
}

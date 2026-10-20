using UnityEngine;

/// <summary>
/// 生成可玩的占位街区：地面、道路、方块楼、玩家、红车、黄色任务点。
/// 编辑器菜单和空场景启动都会走这里，避免两套坐标。
/// </summary>
public static class PrototypeWorldBuilder
{
    public static void Build()
    {
        DestroyByName("City");
        DestroyByName("Player");
        DestroyByName("Car");
        DestroyByName("Checkpoint");

        EnsureSun();
        Camera cam = EnsureCamera();

        var city = new GameObject("City").transform;

        Make(PrimitiveType.Cube, city, "Ground",
            new Vector3(0f, -1f, 40f), new Vector3(200f, 2f, 220f), Quaternion.identity,
            CityPalette.Ground, "Ground", true);

        Make(PrimitiveType.Cube, city, "Road",
            new Vector3(0f, 0.02f, 42f), new Vector3(10f, 0.025f, 170f), Quaternion.identity,
            CityPalette.Road, "Road", false);
        Make(PrimitiveType.Cube, city, "SidewalkLeft",
            new Vector3(-7.5f, 0.015f, 42f), new Vector3(4.4f, 0.02f, 170f), Quaternion.identity,
            CityPalette.Sidewalk, "Sidewalk", false);
        Make(PrimitiveType.Cube, city, "SidewalkRight",
            new Vector3(7.5f, 0.015f, 42f), new Vector3(4.4f, 0.02f, 170f), Quaternion.identity,
            CityPalette.Sidewalk, "Sidewalk", false);

        for (float z = -24f; z <= 78f; z += 7f)
        {
            Make(PrimitiveType.Cube, city, "LaneMark",
                new Vector3(0f, 0.045f, z), new Vector3(0.28f, 0.02f, 2.3f), Quaternion.identity,
                CityPalette.LaneMark, "LaneMark", false, true, 0.45f);
        }

        CreateBuildings(city);
        CreateLamps(city);
        CreateCrates(city);

        GameObject player = CreatePlayer(city);
        GameObject car = CreateCar(city);
        MissionCheckpoint mission = CreateCheckpoint(city);
        Wire(player, car, cam, mission);
    }

    static void CreateBuildings(Transform city)
    {
        // x, z, sx, sy, sz, variant。楼的内边缘离路心至少 12 米，不挡车道。
        float[] rows =
        {
            -18f, 6f, 10f, 12f, 10f, 0f,
            -22f, 30f, 12f, 20f, 12f, 1f,
            -16f, 54f, 8f, 9f, 12f, 2f,
            -21f, 76f, 12f, 16f, 10f, 0f,
            -18f, 108f, 14f, 18f, 12f, 1f,
            18f, 14f, 10f, 14f, 12f, 1f,
            17f, 38f, 8f, 8f, 14f, 2f,
            22f, 60f, 12f, 22f, 10f, 0f,
            18f, 84f, 10f, 12f, 12f, 1f,
            20f, 112f, 14f, 10f, 12f, 2f,
            -42f, 48f, 18f, 30f, 18f, 0f,
            46f, 58f, 18f, 26f, 16f, 1f,
            0f, 136f, 34f, 22f, 14f, 2f,
            -36f, -18f, 16f, 14f, 14f, 1f,
            32f, -14f, 14f, 11f, 16f, 0f
        };

        for (int i = 0; i < rows.Length; i += 6)
        {
            float x = rows[i];
            float z = rows[i + 1];
            float sx = rows[i + 2];
            float sy = rows[i + 3];
            float sz = rows[i + 4];
            int variant = Mathf.RoundToInt(rows[i + 5]);
            var building = Make(PrimitiveType.Cube, city, "Building",
                new Vector3(x, sy * 0.5f, z), new Vector3(sx, sy, sz), Quaternion.identity,
                CityPalette.Building(variant), CityPalette.BuildingMaterial(variant), true);

            float face = x < 0f ? 0.52f : -0.52f;
            Make(PrimitiveType.Cube, building.transform, "Glass",
                new Vector3(face, 0.12f, 0f), new Vector3(0.05f, 0.38f, 0.62f), Quaternion.identity,
                CityPalette.Glass, "Glass", false, false, 0.55f);
        }
    }

    static void CreateLamps(Transform city)
    {
        float[] zs = { -8f, 16f, 40f, 64f, 88f };
        for (int i = 0; i < zs.Length; i++)
        {
            CreateLamp(city, -6.4f, zs[i]);
            CreateLamp(city, 6.4f, zs[i]);
        }
    }

    static void CreateLamp(Transform city, float x, float z)
    {
        Make(PrimitiveType.Cylinder, city, "LampPole",
            new Vector3(x, 2.1f, z), new Vector3(0.16f, 2.1f, 0.16f), Quaternion.identity,
            CityPalette.Lamp, "Lamp", false);
        Make(PrimitiveType.Cube, city, "LampHead",
            new Vector3(x, 4.35f, z), new Vector3(0.7f, 0.12f, 0.28f), Quaternion.identity,
            CityPalette.Headlight, "Headlight", false, true, 0.5f);
    }

    static void CreateCrates(Transform city)
    {
        Make(PrimitiveType.Cube, city, "Crate",
            new Vector3(8.6f, 0.4f, 18f), new Vector3(0.9f, 0.8f, 0.9f), Quaternion.Euler(0f, 18f, 0f),
            CityPalette.Crate, "Crate", true);
        Make(PrimitiveType.Cube, city, "Crate",
            new Vector3(-8.5f, 0.35f, 44f), new Vector3(1.1f, 0.7f, 0.7f), Quaternion.Euler(0f, -12f, 0f),
            CityPalette.Crate, "Crate", true);
        Make(PrimitiveType.Cube, city, "Crate",
            new Vector3(8.4f, 0.45f, 72f), new Vector3(0.8f, 0.9f, 0.8f), Quaternion.identity,
            CityPalette.Crate, "Crate", true);
    }

    static GameObject CreatePlayer(Transform city)
    {
        var player = new GameObject("Player");
        player.transform.SetParent(city, false);
        player.transform.position = new Vector3(0f, 1.02f, -2f);

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

        player.AddComponent<ThirdPersonController>();
        player.AddComponent<PlayerVehicleSwitch>();
        return player;
    }

    static GameObject CreateCar(Transform city)
    {
        var car = new GameObject("Car");
        car.transform.SetParent(city, false);
        car.transform.position = new Vector3(0f, 0.35f, 5f);

        car.AddComponent<Rigidbody>();
        car.AddComponent<VehicleController>();

        var box = car.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.62f, 0f);
        box.size = new Vector3(1.85f, 1.24f, 4.2f);

        Make(PrimitiveType.Cube, car.transform, "Body",
            new Vector3(0f, 0.62f, 0f), new Vector3(1.85f, 0.48f, 4.2f), Quaternion.identity,
            CityPalette.CarBody, "CarBody", false, false, 0.35f);
        Make(PrimitiveType.Cube, car.transform, "Cabin",
            new Vector3(0f, 1.05f, -0.42f), new Vector3(1.6f, 0.45f, 2f), Quaternion.identity,
            CityPalette.CarCabin, "CarCabin", false, false, 0.4f);
        Make(PrimitiveType.Cube, car.transform, "HeadlightL",
            new Vector3(-0.62f, 0.68f, 2.08f), new Vector3(0.28f, 0.16f, 0.08f), Quaternion.identity,
            CityPalette.Headlight, "Headlight", false, true, 0.6f);
        Make(PrimitiveType.Cube, car.transform, "HeadlightR",
            new Vector3(0.62f, 0.68f, 2.08f), new Vector3(0.28f, 0.16f, 0.08f), Quaternion.identity,
            CityPalette.Headlight, "Headlight", false, true, 0.6f);

        CreateWheel(car.transform, "WheelFL", new Vector3(-0.98f, 0.34f, 1.35f));
        CreateWheel(car.transform, "WheelFR", new Vector3(0.98f, 0.34f, 1.35f));
        CreateWheel(car.transform, "WheelRL", new Vector3(-0.98f, 0.34f, -1.35f));
        CreateWheel(car.transform, "WheelRR", new Vector3(0.98f, 0.34f, -1.35f));

        var seat = new GameObject("Seat");
        seat.transform.SetParent(car.transform, false);
        seat.transform.localPosition = new Vector3(0.35f, 0.95f, -0.15f);

        Make(PrimitiveType.Cube, car.transform, "EnterHint",
            new Vector3(0f, 2.55f, 0f), new Vector3(0.42f, 0.42f, 0.42f), Quaternion.Euler(0f, 45f, 0f),
            CityPalette.Checkpoint, "Checkpoint", false, true, 0.5f);

        return car;
    }

    static void CreateWheel(Transform car, string name, Vector3 localPos)
    {
        var hub = new GameObject(name);
        hub.transform.SetParent(car, false);
        hub.transform.localPosition = localPos;
        Make(PrimitiveType.Cylinder, hub.transform, "WheelVisual",
            Vector3.zero, new Vector3(0.68f, 0.12f, 0.68f), Quaternion.Euler(0f, 0f, 90f),
            CityPalette.Wheel, "Wheel", false, false, 0.35f);
    }

    static MissionCheckpoint CreateCheckpoint(Transform city)
    {
        var root = new GameObject("Checkpoint");
        root.transform.SetParent(city, false);
        root.transform.position = new Vector3(0f, 0f, 88f);

        var beacon = Make(PrimitiveType.Cylinder, root.transform, "Beacon",
            new Vector3(0f, 7f, 0f), new Vector3(1.15f, 7f, 1.15f), Quaternion.identity,
            CityPalette.Checkpoint, "Checkpoint", false, true, 0.65f);
        Make(PrimitiveType.Cylinder, root.transform, "Ring",
            new Vector3(0f, 0.06f, 0f), new Vector3(16f, 0.02f, 16f), Quaternion.identity,
            CityPalette.Checkpoint, "Checkpoint", false, true, 0.55f);

        var zone = root.AddComponent<SphereCollider>();
        zone.isTrigger = true;
        zone.center = new Vector3(0f, 2.5f, 0f);
        zone.radius = 8f;

        var mission = root.AddComponent<MissionCheckpoint>();
        mission.beacon = beacon.transform;
        mission.objective = "开车前往黄色光柱";
        return mission;
    }

    static void Wire(GameObject player, GameObject car, Camera cam, MissionCheckpoint mission)
    {
        var motor = player.GetComponent<ThirdPersonController>();
        var sw = player.GetComponent<PlayerVehicleSwitch>();
        var drive = car.GetComponent<VehicleController>();
        var follow = cam.GetComponent<ThirdPersonCamera>();

        motor.view = cam.transform;
        follow.target = player.transform;
        follow.targetOffset = ThirdPersonCamera.OnFootOffset;
        follow.distance = ThirdPersonCamera.OnFootDistance;

        sw.player = motor;
        sw.vehicle = drive;
        sw.cameraRig = follow;
        sw.seat = car.transform.Find("Seat");
        sw.enterHint = car.transform.Find("EnterHint");
        Transform visual = player.transform.Find("Visual");
        if (visual != null)
            sw.hideWhenDriving = visual.GetComponents<Renderer>();

        GameObject systems = GameObject.Find("Game");
        if (systems == null)
            systems = new GameObject("Game");

        if (systems.GetComponent<PrototypeBootstrap>() == null)
            systems.AddComponent<PrototypeBootstrap>();

        MissionHUD hud = systems.GetComponent<MissionHUD>();
        if (hud == null)
            hud = systems.AddComponent<MissionHUD>();

        hud.controls = sw;
        hud.mission = mission;
        hud.vehicle = drive;
        hud.cameraRig = follow;
        if (hud.uiFont == null)
            hud.uiFont = Resources.Load<Font>("UIFont");

        follow.Snap();
    }

    static void EnsureSun()
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

    static Camera EnsureCamera()
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

    static readonly System.Collections.Generic.Dictionary<PrimitiveType, Mesh> MeshCache =
        new System.Collections.Generic.Dictionary<PrimitiveType, Mesh>();

    static GameObject Make(
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

    static Mesh BuiltinMesh(PrimitiveType type)
    {
        Mesh mesh;
        if (MeshCache.TryGetValue(type, out mesh) && mesh != null)
            return mesh;

        GameObject temp = GameObject.CreatePrimitive(type);
        mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        temp.SetActive(false);
        // 立刻拆掉，避免临时碰撞体在车道上停一帧。
        Object.DestroyImmediate(temp);
        MeshCache[type] = mesh;
        return mesh;
    }

    static void AssignEditorMaterial(GameObject go, PrimitiveTint tint)
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

    static void DestroyByName(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
            DestroyObject(go);
    }

    static void DestroyObject(Object obj)
    {
        if (obj == null)
            return;
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>重新生成并验证连通地图；可从菜单或 Unity batchmode 执行。</summary>
public static class CampusSceneValidation
{
    static bool playChecked;
    static double exitAt;

    public static void RunFinalChecks()
    {
        BuildAndValidate();
        RenderPreviews();
        EditorSceneManager.OpenScene("Assets/Scenes/CampusWorld.unity");
        SessionState.SetBool("CampusPlayValidation", true);
        EditorApplication.update += CheckPlayStartup;
        EditorApplication.isPlaying = true;
    }

    [InitializeOnLoadMethod]
    static void RestorePlayCheck()
    {
        if (SessionState.GetBool("CampusPlayValidation", false))
            EditorApplication.update += CheckPlayStartup;
    }

    static void CheckPlayStartup()
    {
        if (!EditorApplication.isPlaying || Time.time < 2f) return;
        if (!playChecked)
        {
            try
            {
                var flow = UnityEngine.Object.FindObjectOfType<CampusFlowController>();
                Require(flow != null && flow.CurrentZone == CampusFlowController.Zone.Exam, "Play 初始区域错误");
                Require(GameProgress.Ensure().stage == GameProgress.StageExam, "Play 进度被其他流程覆盖");
                Require(flow.player.position.y > 4.8f, "Play 出生点落出二楼");
                Require(UnityEngine.Object.FindObjectOfType<CampusHUD>() != null, "Play HUD 缺失");
                Require(flow.hud.objective.Contains("二楼"), "Play 考场目标被覆盖");
                ValidateFirstPerson(flow);
                CampusGameplayValidation.Run(flow);
                File.AppendAllText("../CampusValidation/result.txt", "\nPASS: Play startup; 2F spawn; exam zone/stage/objective; unified HUD present.");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("../CampusValidation/play-hud.png"));
                Debug.Log("CAMPUS_PLAY_VALIDATION_PASS");
                playChecked = true;
                exitAt = EditorApplication.timeSinceStartup + 2;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SessionState.SetBool("CampusPlayValidation", false);
                EditorApplication.Exit(1);
            }
        }
        else if (EditorApplication.timeSinceStartup > exitAt)
        {
            try { if (!CampusGameplayValidation.AdvanceEscape()) return; }
            catch (Exception e) { Debug.LogException(e); SessionState.SetBool("CampusPlayValidation", false); EditorApplication.Exit(1); return; }
            SessionState.SetBool("CampusPlayValidation", false);
            EditorApplication.Exit(0);
        }
    }

    public static void RenderPreviews()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CampusWorld.unity");
        QualitySettings.shadowResolution=UnityEngine.ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance=90f;
        QualitySettings.anisotropicFiltering=AnisotropicFiltering.ForceEnable;
        var camera = Camera.main;
        camera.GetComponent<ThirdPersonCamera>().enabled = false;
        Render(camera, "classroom", new Vector3(-10.6f, 5.65f, -2.8f), new Vector3(-7.4f, 5.3f, 4f));
        Render(camera, "chapter201-clues", new Vector3(-7.5f,5.65f,.1f),new Vector3(-8.3f,5.1f,-3));
        Render(camera, "equipment203",new Vector3(15,5.65f,7),new Vector3(15,5,0));
        Render(camera, "corridor-npcs",new Vector3(-10,5.65f,10),new Vector3(22,5.3f,10));
        Render(camera, "garden", new Vector3(-47f,1.65f,161f), new Vector3(-40f,2.2f,146f));
        Render(camera, "campus-wide", new Vector3(-72f,35f,129f), new Vector3(-5f,0f,154f));
        Render(camera, "stairs", new Vector3(-14.4f, 5.65f, 20.5f), new Vector3(-14.4f, 2.2f, 29f));
        Render(camera, "school-campus", new Vector3(-112f,112f,119f), new Vector3(5f,0f,-30f));
        Render(camera, "school-gate", new Vector3(-33f,2f,72f), new Vector3(-7f,4f,38f));
        Render(camera, "motorcycle", new Vector3(-22.5f,1.35f,61.8f), new Vector3(-19.5f,.7f,59.5f));
        Render(camera, "teaching-building", new Vector3(74,35,52), new Vector3(15,4,-11));
        Render(camera, "open-gallery", new Vector3(3,5.65f,10), new Vector3(37,5.4f,10));
        Render(camera, "hangzhou-town", new Vector3(-108,85,74), new Vector3(5,0,157));
        Render(camera, "hangzhou-street", new Vector3(-56,1.75f,71), new Vector3(-81,4,80));
    }

    static void Render(Camera camera, string name, Vector3 position, Vector3 target)
    {
        camera.transform.position = position;
        camera.transform.LookAt(target);
        var texture = new RenderTexture(1920, 1080, 24) { antiAliasing=4 };
        camera.targetTexture = texture;
        camera.Render();
        RenderTexture.active = texture;
        var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        image.Apply();
        Directory.CreateDirectory("../CampusValidation");
        File.WriteAllBytes("../CampusValidation/" + name + ".png", image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(texture);
    }

    [MenuItem("EscapeExam/验证并生成二楼校园")]
    public static void BuildAndValidate()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        CampusAssetBaker.PrepareTextures();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CampusWorldBuilder.Build();
        CampusAssetBaker.Save(GameObject.Find("LevelRoot").transform);

        Physics.SyncTransforms();
        var player = GameObject.Find("Player");
        Require(player != null && player.transform.position.y > 4.8f, "出生点必须在二楼");
        Require(UnityEngine.Object.FindObjectOfType<CampusHUD>() != null, "统一 HUD 缺失");
        Require(GameObject.Find("ExamExit").transform.position.y > 4f, "考场出口必须在二楼");
        var desk = GameObject.Find("Desk_0");
        Require(desk != null, "课桌缺失");
        Require(Mathf.Abs(desk.transform.Find("Top").localScale.x - 0.7f) < 0.001f, "课桌宽度不正确");
        Require(Mathf.Abs(desk.transform.Find("ChairSeat").localPosition.y - 0.435f) < 0.001f, "座椅高度不正确");
        // 对关键通路逐点检查地板高度和两米站立空间，包含门洞、全部踏步和校门。
        CheckFloor(new Vector3(-7.5f, 4f, -2.2f));
        for (float x = -7.5f; x >= -14.4f; x -= 0.4f) CheckFloor(new Vector3(x, 4f, -1.2f));
        for (float z = -1.2f; z <= 21.8f; z += 0.4f) CheckFloor(new Vector3(-14.4f, 4f, z));
        for (int i = 0; i < 20; i++)
            CheckFloor(new Vector3(-14.4f, 4f - (i + 1) * 0.2f, 22.25f + i * 0.5f));
        for (float z = 32.3f; z <= 51.8f; z += 0.4f) CheckFloor(new Vector3(-14.4f, 0f, z));
        ValidateRoofsAndScale();
        CampusSiteValidation.CheckLayout();
        CampusEscapeValidation.CheckLayout();
        string target = "Assets/Scenes/CampusWorld.unity";
        if (File.Exists(target))
        {
            Directory.CreateDirectory("../CampusValidation");
            if (!File.Exists("../CampusValidation/CampusWorld.before-v07.unity"))
                File.Copy(target, "../CampusValidation/CampusWorld.before-v07.unity");
        }
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), target);
        AssetDatabase.SaveAssets();
        CampusProjectMaintenance.CleanGeneratedResources();
        Directory.CreateDirectory("../CampusValidation");
        File.WriteAllText("../CampusValidation/result.txt", "PASS: spawn 2F; exit 2F; desk/chair metrics; continuous route and standing clearance; scene saved.\n" + DateTime.UtcNow.ToString("O"));
        Debug.Log("CAMPUS_VALIDATION_PASS");
    }

    static void CheckFloor(Vector3 feet)
    {
        RaycastHit hit;
        Require(Physics.Raycast(feet + Vector3.up * 0.12f, Vector3.down, out hit, 0.3f, ~0, QueryTriggerInteraction.Ignore), "通路缺少地板: " + feet);
        Require(Mathf.Abs(hit.point.y - feet.y) < 0.11f, "地板高度错误: " + feet + " actual=" + hit.point.y);
        foreach (var obstacle in Physics.OverlapCapsule(feet + Vector3.up * 0.65f, feet + Vector3.up * 1.7f,
            0.28f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (obstacle.GetComponentInParent<ThirdPersonController>() != null ||
                obstacle.GetComponentInParent<TeacherAgent>() != null ||
                obstacle.name=="Chapter201SideDoor" || obstacle.GetComponent<CampusEscapeObject>()!=null) continue;
            Require(false, "站立通路有障碍: " + feet + " object=" + obstacle.name);
        }
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void ValidateRoofsAndScale()
    {
        var body = GameObject.Find("Player").GetComponent<CharacterController>();
        Require(Mathf.Abs(body.height - 1.8f) < 0.001f, "人物身高必须为 1.8 米");
        var rig = Camera.main.GetComponent<ThirdPersonCamera>();
        Require(rig.IsFirstPerson, "校园必须启用第一人称");
        foreach (var point in new[] {
            new Vector3(-7.5f, 5.65f, -2.2f), new Vector3(-7.5f, 5.65f, 3f),
            new Vector3(-14.4f, 5.65f, 9f), new Vector3(-14.4f, 3.65f, 27f),
            new Vector3(-14.4f, 1.65f, 40f), new Vector3(-28f, 1.65f, 118f) })
        {
            RaycastHit roof;
            Require(Physics.Raycast(point, Vector3.up, out roof, 4f, ~0, QueryTriggerInteraction.Ignore), "室内顶面漏空: " + point);
            Require(roof.collider.name.Contains("Ceiling"), "室内顶面应命中天花板: " + point + " " + roof.collider.name);
        }
        Require(Mathf.Abs(GameObject.Find("ExamHall").transform.Find("Floor").localScale.x - 9f) < 0.01f, "教室宽度错误");
        CheckFloor(new Vector3(-28f, 0f, 115f));
        CheckFloor(new Vector3(-28f, 0f, 119.5f));
    }

    static void ValidateFirstPerson(CampusFlowController flow)
    {
        var motor = flow.player.GetComponent<ThirdPersonController>();
        var body = flow.player.GetComponent<CharacterController>();
        var rig = Camera.main.GetComponent<ThirdPersonCamera>();
        float feet = body.bounds.min.y;
        rig.Snap();
        Require(rig.IsFirstPerson, "Play 第一人称未启用");
        Require(Mathf.Abs(Camera.main.transform.position.y - feet - 1.65f) < 0.025f, "站立眼高不正确");
        Require(motor.TrySetCrouching(true), "无法蹲下");
        Physics.SyncTransforms();
        rig.Snap();
        Require(Mathf.Abs(body.bounds.min.y - feet) < 0.025f, "蹲下脚底发生位移");
        Require(Mathf.Abs(Camera.main.transform.position.y - feet - 0.95f) < 0.025f, "蹲姿眼高不正确");
        var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blocker.transform.position = new Vector3(flow.player.position.x, feet + 1.4f, flow.player.position.z);
        blocker.transform.localScale = new Vector3(1f, 0.2f, 1f);
        Physics.SyncTransforms();
        Require(!motor.TrySetCrouching(false), "低顶下不应允许站起");
        UnityEngine.Object.DestroyImmediate(blocker);
        Physics.SyncTransforms();
        Require(motor.TrySetCrouching(false), "移除低顶后无法站起");
        rig.Snap();
        foreach (var renderer in flow.player.GetComponentsInChildren<Renderer>())
            Require(renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly, "第一人称身体遮挡镜头");
        File.AppendAllText("../CampusValidation/result.txt", "\nPASS: enclosed roofs; human scale; first-person eyes 1.65/0.95m; crouch feet; blocked standing.");
    }

}

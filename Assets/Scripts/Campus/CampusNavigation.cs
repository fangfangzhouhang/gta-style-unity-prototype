using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-150)]
public class CampusNavigation : MonoBehaviour
{
    public NavMeshData data;
    NavMeshDataInstance instance;
    void OnEnable() { if (Application.isPlaying && data != null) instance = NavMesh.AddNavMeshData(data); }
    void Start()
    {
        if (data == null) { data = Bake(transform); instance = NavMesh.AddNavMeshData(data); }
    }
    void OnDisable() { if (instance.valid) instance.Remove(); }

    public static NavMeshData Bake(Transform root)
    {
        var sources = new List<NavMeshBuildSource>();
        var markup = new List<NavMeshBuildMarkup>();
        foreach (var actor in root.GetComponentsInChildren<TeacherAgent>())
            markup.Add(new NavMeshBuildMarkup { root = actor.transform, ignoreFromBuild = true });
        foreach (var actor in root.GetComponentsInChildren<ThirdPersonController>())
            markup.Add(new NavMeshBuildMarkup { root = actor.transform, ignoreFromBuild = true });
        foreach (var bike in root.GetComponentsInChildren<CampusMotorcycle>())
            markup.Add(new NavMeshBuildMarkup { root = bike.transform, ignoreFromBuild = true });
        NavMeshBuilder.CollectSources(root, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markup, sources);
        var settings = NavMesh.GetSettingsByID(0);
        settings.agentRadius = 0.28f;
        settings.agentHeight = 1.8f;
        settings.agentClimb = 0.25f;
        settings.agentSlope = 42f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = 0.07f;
        return NavMeshBuilder.BuildNavMeshData(settings, sources,
            new Bounds(new Vector3(10f, 5f, 40f), new Vector3(240f, 40f, 400f)), Vector3.zero, Quaternion.identity);
    }
}

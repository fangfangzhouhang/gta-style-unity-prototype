using UnityEngine;

/// <summary>
/// 尝试实例化 Assets/Art/ThirdParty 下的 FBX（仅编辑器 AssetDatabase；运行时回退 null）。
/// </summary>
public static class ThirdPartyModelUtil
{
    public static GameObject TrySpawnBuilding(string assetPath, Transform parent, Vector3 position, Quaternion rotation, float height)
    {
        var go=TrySpawn(assetPath,parent,position,rotation,Vector3.one);
        if(go==null) return null;
        var renderers=go.GetComponentsInChildren<Renderer>();
        if(renderers.Length==0) return go;
        Bounds bounds=renderers[0].bounds;
        foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if(bounds.size.y>0.001f) go.transform.localScale*=height/bounds.size.y;
        bounds=renderers[0].bounds;
        foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        go.transform.position+=new Vector3(position.x-bounds.center.x,position.y-bounds.min.y,position.z-bounds.center.z);
        bounds=renderers[0].bounds;
        foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if(go.GetComponentInChildren<Collider>()==null)
        {
            var proxy=new GameObject("BuildingCollision");
            proxy.transform.position=bounds.center;
            proxy.transform.SetParent(go.transform,true);
            var collider=proxy.AddComponent<BoxCollider>();
            collider.size=bounds.size;
        }
        return go;
    }
    public const string FurnitureDesk = "Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/desk.fbx";
    public const string FurnitureChairDesk = "Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/chairDesk.fbx";
    public const string FurnitureBookcase = "Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/bookcaseOpen.fbx";
    public const string FurnitureDoorway = "Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/doorwayOpen.fbx";
    public const string FurnitureCabinet = "Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/cabinetBed.fbx";
    public const string RoadStraight = "Assets/Art/ThirdParty/Kenney/CityKitRoads/Models/road-straight.fbx";
    public const string RoadCrossing = "Assets/Art/ThirdParty/Kenney/CityKitRoads/Models/road-crossing.fbx";
    public const string BuildingA = "Assets/Art/ThirdParty/Kenney/CityKitSuburban/Models/building-type-a.fbx";
    public const string BuildingB = "Assets/Art/ThirdParty/Kenney/CityKitSuburban/Models/building-type-b.fbx";
    public const string BuildingC = "Assets/Art/ThirdParty/Kenney/CityKitSuburban/Models/building-type-c.fbx";
    public const string TreeSmall = "Assets/Art/ThirdParty/Kenney/CityKitSuburban/Models/tree-small.fbx";
    public const string Classroom = "Assets/Art/ThirdParty/Grab3D/Classroom/classroom-light.fbx";
    public const string SchoolBuilding = "Assets/Art/ThirdParty/Grab3D/SchoolBuilding/school-building-light.fbx";

    public static GameObject TrySpawn(string assetPath, Transform parent, Vector3 worldPos, Quaternion worldRot, Vector3 scale)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return null;

        Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (obj == null)
            return null;

        GameObject source = obj as GameObject;
        if (source == null)
        {
            // FBX 主资源可能是带 MeshFilter 的根
            source = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
        if (source == null)
            return null;

        GameObject go = UnityEditor.PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (go == null)
            go = Object.Instantiate(source);
        if (go == null)
            return null;

        if (parent != null)
            go.transform.SetParent(parent, true);
        go.transform.position = worldPos;
        go.transform.rotation = worldRot;
        go.transform.localScale = scale;
        go.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        return go;
#else
        return null;
#endif
    }

    public static bool HasAsset(string assetPath)
    {
#if UNITY_EDITOR
        return !string.IsNullOrEmpty(UnityEditor.AssetDatabase.AssetPathToGUID(assetPath));
#else
        return false;
#endif
    }
}

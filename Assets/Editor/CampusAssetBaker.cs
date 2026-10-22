using UnityEngine;
using UnityEditor;

public static class CampusAssetBaker
{
    public static void PrepareTextures()
    {
        foreach (string name in new[] { "bark_brown_02_nor_gl", "leafy_grass_nor_gl" })
        {
            var importer = AssetImporter.GetAtPath("Assets/Resources/CampusArt/"+name+".jpg") as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            { importer.textureType=TextureImporterType.NormalMap; importer.SaveAndReimport(); }
        }
        CampusPresentationSettings.RestoreGameView();
    }

    public static void Save(Transform root)
    {
        const string directory="Assets/Art/GeneratedCampus";
        if (!AssetDatabase.IsValidFolder(directory)) AssetDatabase.CreateFolder("Assets/Art","GeneratedCampus");
        foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh=filter.sharedMesh;
            if (mesh==null || !mesh.name.StartsWith("CampusArt_") || AssetDatabase.Contains(mesh)) continue;
            string path=directory+"/"+mesh.name+".asset";
            Mesh saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved==null) { AssetDatabase.CreateAsset(mesh,path); saved=mesh; }
            else if(saved!=mesh) { EditorUtility.CopySerialized(mesh,saved); EditorUtility.SetDirty(saved); }
            filter.sharedMesh=saved;
            var collider=filter.GetComponent<MeshCollider>();
            if(collider!=null && collider.sharedMesh==mesh)collider.sharedMesh=saved;
        }
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            Material material=renderer.sharedMaterial;
            if (material==null || !material.name.StartsWith("CampusArt_") || AssetDatabase.Contains(material)) continue;
            string path=directory+"/"+material.name+".mat";
            Material saved=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (saved==null) { AssetDatabase.CreateAsset(material,path); saved=material; }
            else if(saved!=material) { EditorUtility.CopySerialized(material,saved); EditorUtility.SetDirty(saved); }
            renderer.sharedMaterial=saved;
        }
        var navigation=root.GetComponent<CampusNavigation>();
        if (navigation!=null)
        {
            var data=CampusNavigation.Bake(root);
            string path=directory+"/CampusNavMesh.asset";
            var saved=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(path);
            if (saved==null) { AssetDatabase.CreateAsset(data,path); saved=data; }
            else { EditorUtility.CopySerialized(data,saved); Object.DestroyImmediate(data); EditorUtility.SetDirty(saved); }
            navigation.data=saved;
        }
        AssetDatabase.SaveAssets();
    }
}

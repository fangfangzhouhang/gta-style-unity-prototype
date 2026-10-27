using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Delete only generator-owned assets with no dependency from any retained project asset.</summary>
public static class CampusProjectMaintenance
{
    [Serializable] public class CleanupReport {public string date;public string[] deleted;}
    [MenuItem("EscapeExam/维护/清理无引用校园生成资源")]
    public static void CleanGeneratedResources()
    {
        const string generated="Assets/Art/GeneratedCampus/";
        var roots=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/") && !p.StartsWith(generated) &&
            new[]{".unity",".prefab",".mat",".asset",".controller",".anim"}.Contains(Path.GetExtension(p))).ToArray();
        var used=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
        var removed=new List<string>();
        string archive=Path.GetFullPath("../CampusValidation/CleanupArchive/"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        foreach(string file in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith(generated) && !AssetDatabase.IsValidFolder(p)).ToArray())
        {
            string name=Path.GetFileName(file);
            if(used.Contains(file) || !(name.StartsWith("CampusArt_") || name=="CampusNavMesh.asset"))continue;
            string backup=Path.Combine(archive,name);Directory.CreateDirectory(archive);
            File.Copy(file,backup,true);if(File.Exists(file+".meta"))File.Copy(file+".meta",backup+".meta",true);
            if(AssetDatabase.DeleteAsset(file))removed.Add(file);
        }
        Directory.CreateDirectory("../CampusValidation");
        File.WriteAllText("../CampusValidation/cleanup-v11.json",JsonUtility.ToJson(new CleanupReport{date=DateTime.UtcNow.ToString("O"),deleted=removed.ToArray()},true));
        Debug.Log("CAMPUS_CLEANUP_PASS removed="+removed.Count);
    }
}

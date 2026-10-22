using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>进入校园 Play 时使用干净的 Game 画面，导航仅参与寻路。</summary>
[InitializeOnLoad]
public static class CampusPresentationSettings
{
    const BindingFlags StaticFlags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    const BindingFlags InstanceFlags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static double captureAt;
    static readonly Type gameViewType=typeof(Editor).Assembly.GetType("UnityEditor.GameView");

    static CampusPresentationSettings()
    {
        EditorApplication.delayCall+=ApplyForCampus;
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode || state==PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall+=ApplyForCampus;
        };
        EditorSceneManager.sceneOpened+=(scene,mode)=>EditorApplication.delayCall+=ApplyForCampus;
    }

    static void ApplyForCampus()
    {
        if(SceneManager.GetActiveScene().name!="CampusWorld") return;
        RestoreGameView();
        captureAt=EditorApplication.timeSinceStartup+1.5;
        EditorApplication.update-=CaptureAfterRepaint;
        EditorApplication.update+=CaptureAfterRepaint;
    }

    [MenuItem("EscapeExam/显示/恢复正常游戏画面（隐藏调试网格）")]
    public static void RestoreGameView()
    {
        Type navType=typeof(UnityEditor.AI.NavMeshVisualizationSettings);
        foreach(string property in new[] {"showNavMesh","showHeightMesh","showNavMeshPortals","showNavMeshLinks",
            "showProximityGrid","showHeightMeshBVTree","showHeightMaps","showAgentPath","showAgentPathInfo",
            "showAgentNeighbours","showAgentWalls","showAgentAvoidance","showObstacleCarveHull"})
            SetBoolean(navType,null,property,false,StaticFlags);
        foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type physicsType=assembly.GetType("UnityEditor.PhysicsVisualizationSettings");
            if(physicsType==null) continue;
            SetBoolean(physicsType,null,"showCollisionGeometry",false,StaticFlags);
            SetBoolean(physicsType,null,"showContacts",false,StaticFlags);
        }
        GizmoUtility.SetIconEnabled(typeof(Light),false);
        if(gameViewType!=null) foreach(var view in Resources.FindObjectsOfTypeAll(gameViewType))
        {
            SetBoolean(gameViewType,view,"drawGizmos",false,InstanceFlags);
            ((EditorWindow)view).Repaint();
        }
        SceneView.RepaintAll();
        WriteStatus();
    }

    static void SetBoolean(Type type,object instance,string name,bool value,BindingFlags flags)
    {
        var property=type.GetProperty(name,flags);
        if(property!=null && property.PropertyType==typeof(bool) && property.CanWrite)
            property.SetValue(instance,value,null);
    }

    static string OutputPath(string file)
    {
        string project=Path.GetDirectoryName(Application.dataPath);
        string directory=Path.GetFullPath(Path.Combine(project,"..","CampusValidation"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory,file);
    }

    static void WriteStatus()
    {
        var nav=typeof(UnityEditor.AI.NavMeshVisualizationSettings).GetProperty("showNavMesh",StaticFlags);
        string status="Scene="+SceneManager.GetActiveScene().name+"\nNavMesh overlay="+nav?.GetValue(null,null)+"\n";
        if(gameViewType!=null) foreach(var view in Resources.FindObjectsOfTypeAll(gameViewType))
            status+="GameView gizmos="+gameViewType.GetProperty("drawGizmos",InstanceFlags)?.GetValue(view,null)+"\n";
        status+="Light component icons hidden; catcher bulb markers are drawn by ScrutinyAura.OnGUI.\n";
        File.WriteAllText(OutputPath("presentation-status.txt"),status);
    }

    static void CaptureAfterRepaint()
    {
        if(EditorApplication.timeSinceStartup<captureAt) return;
        EditorApplication.update-=CaptureAfterRepaint;
        if(gameViewType==null) return;
        foreach(var view in Resources.FindObjectsOfTypeAll(gameViewType))
        {
            var texture=gameViewType.GetField("m_RenderTexture",InstanceFlags)?.GetValue(view) as RenderTexture;
            if(texture==null || !texture.IsCreated()) continue;
            var previous=RenderTexture.active;
            RenderTexture.active=texture;
            var pixels=new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
            pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
            pixels.Apply();
            File.WriteAllBytes(OutputPath("presentation-gameview.png"),pixels.EncodeToPNG());
            RenderTexture.active=previous;
            UnityEngine.Object.DestroyImmediate(pixels);
            break;
        }
    }

    public static void Validate()
    {
        // 重现截图里的编辑状态，然后验证恢复逻辑；不修改场景几何或导航资产。
        Type navType=typeof(UnityEditor.AI.NavMeshVisualizationSettings);
        SetBoolean(navType,null,"showNavMesh",true,StaticFlags);
        EditorWindow view=EditorWindow.GetWindow(gameViewType);
        SetBoolean(gameViewType,view,"drawGizmos",true,InstanceFlags);
        RestoreGameView();
        var overlay=navType.GetProperty("showNavMesh",StaticFlags);
        if((bool)overlay.GetValue(null,null)) throw new InvalidOperationException("导航覆盖层仍开启");
        if((bool)gameViewType.GetProperty("drawGizmos",InstanceFlags).GetValue(view,null))
            throw new InvalidOperationException("Game 视图仍显示 Gizmos");
        if(typeof(ScrutinyAura).GetMethod("OnGUI",InstanceFlags)==null)
            throw new InvalidOperationException("抓捕者灯光标志缺失");
        Debug.Log("CAMPUS_PRESENTATION_VALIDATION_PASS");
    }
}

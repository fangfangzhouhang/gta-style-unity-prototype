using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Additive study: never calls CampusWorldBuilder or saves CampusWorld.
public static class ArtLab201Builder
{
    const string Output = "Docs/ArtLab201";
    [MenuItem("EscapeExam/ArtLab 201/Create isolated study")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory("Assets/Art/ArtLab201");
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/CampusWorld.unity");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/ArtLab_201.unity");
        Capture("before");
        var exam=GameObject.Find("ExamHall").transform;
        // In the source layout 202's west wall sits one metre outside 201's windows.
        // The art study uses an open light court here; production layout is untouched.
        var adjacent=GameObject.Find("Classroom_202");
        if(adjacent!=null)adjacent.SetActive(false);
        var wood=MakeMaterial("Wood",new Color(.38f,.26f,.15f),.28f);
        var paint=MakeMaterial("Paint",new Color(.24f,.30f,.28f),.22f);
        int count=0;
        foreach(Transform desk in exam)
        {
            if(!desk.name.StartsWith("Desk_"))continue;
            Fit("desk",desk,Vector3.zero,new Vector3(.70f,.76f,.50f),wood);
            Fit("chair",desk,new Vector3(0,0,-.72f),new Vector3(.40f,.84f,.44f),paint);
            foreach(Transform child in desk)
            {
                if(child.name.StartsWith("ArtLab_") || child.name=="ExamPaper")continue;
                var r=child.GetComponent<Renderer>();if(r!=null)r.enabled=false;
            }
            count++;
        }
        // Preserve original collision blockers while letting direct light through.
        foreach(var r in exam.GetComponentsInChildren<MeshRenderer>())
            if(r.name=="WindowGlass")r.enabled=false;
        var probe=new GameObject("ArtLab201_LightProbes").AddComponent<LightProbeGroup>();
        probe.transform.SetParent(exam,false);
        var points=new List<Vector3>();
        foreach(float x in new[]{-11f,-8f,-4f})foreach(float z in new[]{-2f,1f,4f,7f})
            foreach(float y in new[]{.4f,1.7f,2.9f})points.Add(new Vector3(x,y,z));
        probe.probePositions=points.ToArray();
        var reflection=new GameObject("ArtLab201_Reflection").AddComponent<ReflectionProbe>();
        reflection.transform.position=new Vector3(-7.5f,5.7f,2);
        reflection.size=new Vector3(9,3.4f,12);reflection.boxProjection=true;
        reflection.mode=ReflectionProbeMode.Realtime;reflection.refreshMode=ReflectionProbeRefreshMode.OnAwake;
        reflection.resolution=128;
        for(int i=0;i<3;i++){Lighting(i);Capture(new[]{"day","dusk","suspense"}[i]);}
        Lighting(0);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/ArtLab_201.unity");
        AssetDatabase.SaveAssets();
        File.WriteAllText(Output+"/build-result.txt","Unity="+Application.unityVersion+"\nTemporary furniture sets="+count+"\nEditor render only; no Play/Profiler/character acceptance. Baking NOT executed.\n");
        Debug.Log("ARTLAB201_BUILD_PASS");
    }
    static Material MakeMaterial(string name,Color color,float smoothness)
    {
        string path="Assets/Art/ArtLab201/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Glossiness",smoothness);return m;
    }
    static void Fit(string model,Transform parent,Vector3 at,Vector3 size,Material mat)
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/ThirdParty/Kenney/FurnitureKit/Models/"+model+".fbx");
        if(source==null)throw new InvalidOperationException("Missing temporary licensed model: "+model);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(source);
        go.name="ArtLab_Temporary_"+model;go.transform.SetParent(parent,false);
        var renderers=go.GetComponentsInChildren<Renderer>();
        if(renderers.Length==0)throw new InvalidOperationException("No mesh: "+model);
        var bounds=renderers[0].bounds;
        foreach(var r in renderers){bounds.Encapsulate(r.bounds);r.sharedMaterial=mat;}
        if(bounds.size.x<.001f||bounds.size.y<.001f||bounds.size.z<.001f)throw new InvalidOperationException("Invalid bounds");
        go.transform.localScale=new Vector3(size.x/bounds.size.x,size.y/bounds.size.y,size.z/bounds.size.z);
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        go.transform.position+=parent.TransformPoint(at)-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
    }
    public static void Lighting(int mode)
    {
        var sun=RenderSettings.sun;
        sun.transform.rotation=Quaternion.Euler(mode==1?14:32,-65,0);
        sun.color=mode==1?new Color(1,.64f,.36f):new Color(1,.93f,.80f);
        sun.intensity=mode==2?.58f:mode==1?.85f:1.15f;sun.shadows=LightShadows.Soft;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.42f,.47f,.53f);
        RenderSettings.ambientEquatorColor=new Color(.29f,.33f,.38f);
        RenderSettings.ambientGroundColor=new Color(.16f,.18f,.21f);
        foreach(var l in UnityEngine.Object.FindObjectsOfType<Light>())
            if(l.type==LightType.Point&&l.transform.IsChildOf(GameObject.Find("ExamHall").transform))
            {l.color=new Color(.70f,.82f,1);l.intensity=mode==2?.8f:1.35f;}
    }
    static void Capture(string variant)
    {
        var c=Camera.main;var follow=c.GetComponent<ThirdPersonCamera>();
        bool enabled=follow!=null&&follow.enabled;if(follow!=null)follow.enabled=false;
        var pos=c.transform.position;var rot=c.transform.rotation;
        c.transform.position=new Vector3(-10.6f,5.65f,-2.8f);c.transform.LookAt(new Vector3(-7.4f,5.3f,4));
        var rt=RenderTexture.GetTemporary(1920,1080,24);var old=c.targetTexture;var active=RenderTexture.active;
        try{
            c.targetTexture=rt;c.Render();RenderTexture.active=rt;
            var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();
            File.WriteAllBytes(Output+"/"+variant+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
        }finally{c.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);c.transform.SetPositionAndRotation(pos,rot);if(follow!=null)follow.enabled=enabled;}
    }
}

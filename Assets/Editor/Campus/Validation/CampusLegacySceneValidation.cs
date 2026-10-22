using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Short startup regression in existing standalone scenes; never saves scene assets.</summary>
public static class CampusLegacySceneValidation
{
    const string Active="CampusLegacyValidation";
    static readonly string[] scenes={"OpenWorld","ExamHall","SchoolEscape","Home","HubTown","TransitHub","TravelMissions"};
    static bool checkedPlay;
    [InitializeOnLoadMethod]
    static void Resume()
    {
        if(!SessionState.GetBool(Active,false))return;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
        Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;
    }
    public static void RunChecks()
    {
        SessionState.SetBool("CampusPlayValidation",false);SessionState.SetBool(Active,true);
        SessionState.SetInt("CampusLegacyIndex",0);SessionState.SetBool("CampusLegacyError",false);
        File.WriteAllText("../CampusValidation/legacy-result.txt","Legacy startup smoke tests; 3 simulated seconds per scene.\n");
        Resume();OpenNext();
    }
    static void OpenNext()
    {
        int index=SessionState.GetInt("CampusLegacyIndex",0);
        if(index>=scenes.Length){SessionState.SetBool(Active,false);Debug.Log("CAMPUS_LEGACY_STARTUP_PASS scenes="+scenes.Length);EditorApplication.Exit(0);return;}
        checkedPlay=false;EditorSceneManager.OpenScene("Assets/Scenes/"+scenes[index]+".unity");EditorApplication.isPlaying=true;
    }
    static void Log(string message,string stack,LogType type)
    {
        if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert)SessionState.SetBool("CampusLegacyError",true);
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || checkedPlay || Time.time<3)return;
        checkedPlay=true;
        int index=SessionState.GetInt("CampusLegacyIndex",0);
        bool valid=!SessionState.GetBool("CampusLegacyError",false) && SceneManager.GetActiveScene().name==scenes[index] && Camera.main!=null && UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>()==null;
        var bike=UnityEngine.Object.FindObjectOfType<CampusMotorcycle>();if(bike!=null)valid &= bike.RideUnlocked;
        if(!valid){SessionState.SetBool(Active,false);Debug.LogError("Legacy startup failed: "+scenes[index]);EditorApplication.Exit(1);return;}
        File.AppendAllText("../CampusValidation/legacy-result.txt","PASS: "+scenes[index]+" Play startup; camera; no runtime errors during smoke window; new chapter absent.\n");
        Debug.Log("CAMPUS_LEGACY_SCENE_PASS "+scenes[index]);EditorApplication.isPlaying=false;
    }
    static void Changed(PlayModeStateChange change)
    {
        if(change!=PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Active,false))return;
        SessionState.SetInt("CampusLegacyIndex",SessionState.GetInt("CampusLegacyIndex",0)+1);
        EditorApplication.delayCall+=OpenNext;
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Runs in a visible validation editor: batch mode does not produce an IMGUI Game screenshot.</summary>
public static class CampusJournalPreviewValidation
{
    const string Flag="CampusJournalPreview";
    static double deadline, captureAt;
    static bool requested;
    [InitializeOnLoadMethod]
    static void Resume(){if(SessionState.GetBool(Flag,false)){deadline=EditorApplication.timeSinceStartup+30;EditorApplication.update+=Tick;}}
    public static void RunPreview()
    {
        SessionState.SetBool("CampusPlayValidation",false);SessionState.SetBool("CampusStage5Validation",false);SessionState.SetBool(Flag,true);
        Directory.CreateDirectory("../CampusValidation");string capture="../CampusValidation/journal-initial.png";if(File.Exists(capture))File.Delete(capture);EditorSceneManager.OpenScene("Assets/Scenes/CampusWorld.unity");
        var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);view.maximized=true;view.Focus();
        Resume();EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying)return;
        if(EditorApplication.timeSinceStartup>deadline){SessionState.SetBool(Flag,false);Debug.LogError("Journal screenshot timed out");EditorApplication.Exit(1);return;}
        if(!requested)
        {
            if(Time.time<2)return;
            var journal=UnityEngine.Object.FindObjectOfType<CampusJournal>();if(journal==null || !journal.Open())return;
            journal.RequestHint();captureAt=EditorApplication.timeSinceStartup+1;requested=true;return;
        }
        if(EditorApplication.timeSinceStartup<captureAt)return;
        string path=Path.GetFullPath("../CampusValidation/journal-initial.png");
        if(!File.Exists(path)){ScreenCapture.CaptureScreenshot(path);captureAt=EditorApplication.timeSinceStartup+.5;return;}
        Debug.Log("CAMPUS_JOURNAL_PREVIEW_PASS "+path);SessionState.SetBool(Flag,false);EditorApplication.Exit(0);
    }
}

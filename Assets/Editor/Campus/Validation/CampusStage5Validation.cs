using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Separate Play suite: real SaveSystem.LoadAndApply(true), not a JSON-only roundtrip.</summary>
public static class CampusStage5Validation
{
    const string Flag="CampusStage5Validation";
    static int index=-1, screenshotFrames;
    static bool loading, done;
    static double deadline;
    static Vector3 expected;
    static string expectedCheckpoint;
    static readonly string[] names={"201","corridor","203","library","service","yard","outside","riding","old-unescaped","old-escaped","old-v12-partial","invalid-locked-pose","completed"};
    [InitializeOnLoadMethod]
    static void Resume(){if(SessionState.GetBool(Flag,false)){deadline=EditorApplication.timeSinceStartup+180;EditorApplication.update+=Tick;}}
    public static void RunChecks()
    {
        Directory.CreateDirectory("../CampusValidation");File.WriteAllText("../CampusValidation/stage5-result.txt","Stage 5: actual scene-reload and modal UI checks\n");
        SessionState.SetBool("CampusPlayValidation",false);SessionState.SetBool("CampusLegacyValidation",false);SessionState.SetBool(Flag,true);
        EditorSceneManager.OpenScene("Assets/Scenes/CampusWorld.unity");EditorApplication.update+=Tick;deadline=EditorApplication.timeSinceStartup+180;EditorApplication.isPlaying=true;
    }
    static void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);}
    static void Pass(string message){File.AppendAllText("../CampusValidation/stage5-result.txt","PASS: "+message+"\n");Debug.Log("CAMPUS_STAGE5_PASS "+message);}
    static void Tick()
    {
        if(done || !EditorApplication.isPlaying || Time.time<1)return;
        try
        {
            Check(EditorApplication.timeSinceStartup<deadline,"stage5 suite timeout case="+index);
            var flow=UnityEngine.Object.FindObjectOfType<CampusFlowController>();if(flow==null)return;
            if(index<0)
            {
                Initial(flow);index=0;screenshotFrames=2;
                UnityEngine.Object.FindObjectOfType<CampusJournal>().Open();
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("../CampusValidation/journal-initial.png"));return;
            }
            if(screenshotFrames>0){screenshotFrames--;if(screenshotFrames==0)UnityEngine.Object.FindObjectOfType<CampusJournal>().Close();return;}
            if(loading)
            {
                var session=UnityEngine.Object.FindObjectOfType<CampusSaveRestore>();if(session==null || !session.Restored)return;
                Verify(flow);index++;loading=false;
                if(index==names.Length){Pass("all 13 actual scene reload cases complete");SessionState.SetBool(Flag,false);done=true;EditorApplication.Exit(0);return;}
            }
            Prepare(flow);loading=true;
        }
        catch(Exception e){Debug.LogException(e);File.AppendAllText("../CampusValidation/stage5-result.txt","FAIL: "+e.Message+"\n");SessionState.SetBool(Flag,false);done=true;EditorApplication.Exit(1);}
    }
    static void Initial(CampusFlowController flow)
    {
        var g=GameProgress.Ensure();var journal=UnityEngine.Object.FindObjectOfType<CampusJournal>();var narration=UnityEngine.Object.FindObjectOfType<CampusNarrative>();var camera=UnityEngine.Object.FindObjectOfType<ThirdPersonCamera>();
        Check(journal!=null && narration!=null,"stage5 components missing in saved scene");
        Check(!journal.Facts.Contains("416") && !journal.Facts.Contains("086") && !journal.Facts.Contains("2、1、3"),"journal leaked undiscovered clue");
        Check(journal.RequestHint().Contains("第 1 级") && journal.RequestHint().Contains("第 2 级") && journal.RequestHint().Contains("第 3 级"),"hint progression failed");
        Check(!g.examChapter.paper && !g.examChapter.chart,"hints changed discovery flags");
        Check(journal.Open() && Time.timeScale==0 && !flow.player.GetComponent<ThirdPersonController>().controlEnabled,"journal did not pause controls");
        Check(!camera.TryCapturePointerOnClick() && Cursor.lockState==CursorLockMode.None,"journal pointer stolen");PauseMenu.Instance.Open();Check(!PauseMenu.Instance.IsOpen,"pause stacked over journal");
        g.mapUnlocked=true;flow.mapUi.Open();Check(!flow.mapUi.IsOpen,"map stacked over journal");g.mapUnlocked=false;Check(journal.HandleEscape(),"journal Escape handler failed");PauseMenu.Instance.Open();Check(!PauseMenu.Instance.IsOpen,"same-frame Escape stacked pause");
        Check(Time.timeScale==1 && flow.player.GetComponent<ThirdPersonController>().controlEnabled,"journal did not restore controls");
        narration.Clear();Check(narration.Emit("validation-once",CampusVoice.Inner,"一次事件") && !narration.Emit("validation-once",CampusVoice.Inner,"重复事件"),"once event repeated");
        narration.Emit(null,CampusVoice.NPC,"重复线索",7,false);narration.Skip();Check(narration.CurrentText=="重复线索","subtitle skip/queue failed");narration.Clear();g.narrative.played.Remove("validation-once");
        g.examChapter.paper=true;Check(journal.Facts.Contains("2、1、3")&&!journal.Facts.Contains("086"),"discovery-driven fact list failed");g.examChapter.paper=false;
        Check(!PauseMenu.Instance.TrySwitchMode("HubTown"),"pause menu bypassed unfinished chapter");
        Pass("journal hides undiscovered facts; three requested hints; pause/map/camera input ownership; subtitle queue/skip and once event");
    }
    static SaveData Fixture(int level)
    {
        var d=new SaveData{version=2,sceneName="CampusWorld",money=75,examChapter=new CampusExamState{introduction=true},corridorChapter=new CampusCorridorState(),libraryChapter=new CampusLibraryState(),departureChapter=new CampusDepartureState(),journal=new CampusJournalState(),narrative=new CampusNarrativeState()};
        d.narrative.played.Add("intro");
        if(level>=1){d.examChapter.paper=d.examChapter.chart=d.examChapter.key=d.examChapter.sideOpen=d.examChapter.corridorIntroduction=true;}
        if(level>=2){d.corridorChapter.notice=d.corridorChapter.conversation=d.corridorChapter.card=d.corridorChapter.roomOpen=d.corridorChapter.record=true;}
        if(level>=3){d.corridorChapter.circuit=d.corridorChapter.lighting=d.corridorChapter.doorPower=true;d.corridorChapter.alarm=false;}
        if(level>=4){d.libraryChapter.indexRead=d.libraryChapter.recordRead=d.libraryChapter.manual=d.libraryChapter.key=d.libraryChapter.downOpen=d.libraryChapter.utilityOpen=true;}
        if(level>=5)d.libraryChapter.emergency=true;
        if(level>=6){d.departureChapter.gateOpen=d.departureChapter.departed=true;d.escapedSchool=d.mapUnlocked=true;}
        return d;
    }
    static void Prepare(CampusFlowController flow)
    {
        int level=Mathf.Min(index,6);var d=Fixture(level);
        Vector3[] positions={new Vector3(-7.5f,4.98f,-2.2f),new Vector3(-14.4f,4.98f,-1.2f),new Vector3(15,4.98f,6),new Vector3(27,8.98f,6.6f),new Vector3(3,.98f,5),new Vector3(16,1.03f,20),new Vector3(-14,1,55)};
        expected=positions[level];expectedCheckpoint=level==0?"ExamCheckpoint":level<3?"CorridorCheckpoint":level==3?"LibraryCheckpoint":level<6?"EastGroundCheckpoint":"HubCheckpoint";
        d.checkpointId=expectedCheckpoint;d.hasPlayerPose=true;d.playerX=expected.x;d.playerY=expected.y;d.playerZ=expected.z;d.playerYaw=42;
        d.journal.visited.Add(level>=3?"library":"201");d.narrative.played.Add("validation-persist");
        if(index==7){d=Fixture(6);d.checkpointId="HubCheckpoint";expected=flow.hubCheckpoint.position;expectedCheckpoint="HubCheckpoint";}
        if(index==8){d=new SaveData{sceneName="CampusWorld",stage=GameProgress.StageSchool,money=123,hasPhone=true,leftNote=true};expected=flow.examCheckpoint.position;expectedCheckpoint="ExamCheckpoint";}
        if(index==9){d=new SaveData{sceneName="CampusWorld",escapedSchool=true,money=123,hasPhone=true,leftNote=true};expected=flow.hubCheckpoint.position;expectedCheckpoint="HubCheckpoint";}
        if(index==10){d=Fixture(3);d.version=0;d.journal=null;d.narrative=null;d.checkpointId="LibraryCheckpoint";expected=GameObject.Find("LibraryCheckpoint").transform.position;expectedCheckpoint="LibraryCheckpoint";}
        if(index==11){d=Fixture(0);d.checkpointId="LibraryCheckpoint";d.hasPlayerPose=true;d.playerX=27;d.playerY=8.98f;d.playerZ=6;expected=flow.examCheckpoint.position;expectedCheckpoint="ExamCheckpoint";}
        if(index==12){d=Fixture(6);d.departureChapter.completed=true;d.narrative.played.Add("complete");d.checkpointId="HubCheckpoint";d.hasPlayerPose=true;expected=new Vector3(-8,.98f,86);d.playerX=expected.x;d.playerY=expected.y;d.playerZ=expected.z;expectedCheckpoint="HubCheckpoint";}
        if(index<=7 || index==12)
        {
            GameProgress.Ensure().ApplyFromSave(d);
            var body=flow.player.GetComponent<CharacterController>();body.enabled=false;flow.player.position=index==7?new Vector3(-20.9f,1.05f,59.5f):expected;body.enabled=true;
            flow.activeCheckpoint=GameObject.Find(expectedCheckpoint).transform;
            if(index<=6 || index==12){flow.player.rotation=Quaternion.Euler(0,42,0);UnityEngine.Object.FindObjectOfType<ThirdPersonCamera>().RestoreLook(42);}
            if(index==7){Physics.SyncTransforms();Check(UnityEngine.Object.FindObjectOfType<CampusMotorcycle>().TryMount(),"riding save fixture mount failed");}
            SaveSystem.Save();
        }
        else File.WriteAllText(SaveSystem.FilePath,JsonUtility.ToJson(d));
        Check(SaveSystem.LoadAndApply(true),"actual load failed case="+names[index]);
    }
    static void Verify(CampusFlowController flow)
    {
        var g=GameProgress.Ensure();var departure=UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>();var library=UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>();
        Check(Vector3.Distance(flow.player.position,expected)<.4f,"wrong restored position case="+names[index]+" actual="+flow.player.position+" expected="+expected);
        Check(flow.activeCheckpoint.name==expectedCheckpoint,"wrong checkpoint id");
        if(index<=6 || index==12)Check(Mathf.Abs(Mathf.DeltaAngle(flow.player.eulerAngles.y,42))<.5f && Mathf.Abs(Mathf.DeltaAngle(UnityEngine.Object.FindObjectOfType<ThirdPersonCamera>().transform.eulerAngles.y,42))<.5f,"restored camera/player yaw differs");Check(g.ToSaveData().version==2,"missing schema version");
        Check(departure.gate.GetComponent<Collider>().enabled==!g.departureChapter.gateOpen,"gate collider differs after scene reload");
        Check(library.yardDoor.GetComponent<Collider>().enabled==!g.libraryChapter.emergency,"yard state differs after scene reload");
        Check(UnityEngine.Object.FindObjectOfType<CampusMotorcycle>().RideUnlocked==g.departureChapter.departed,"ride permission differs after reload");
        Check(!UnityEngine.Object.FindObjectOfType<CampusMotorcycle>().IsRiding,"riding load did not recover on foot");
        if(index<=6){Check(g.narrative.played.Contains("validation-persist"),"narrative history lost");Check(g.journal.visited.Count>0,"journal lost");}
        if(index==8 || index==9)Check(g.money==123 && g.hasPhone && g.leftNote,"migration lost economy");
        if(index==9)Check(g.departureChapter.completed && !departure.gate.GetComponent<Collider>().enabled,"old completed save trapped");
        if(index==12)Check(PauseMenu.Instance.CanChangeMode && g.departureChapter.completed && !UnityEngine.Object.FindObjectOfType<CampusNarrative>().Emit("complete",CampusVoice.Inner,"重复结尾"),"completed state/event lost");
        if(index==11)Check(!g.examChapter.sideOpen && flow.CurrentZone==CampusFlowController.Zone.Exam,"invalid pose bypassed lock");
        var narrative=UnityEngine.Object.FindObjectOfType<CampusNarrative>();Check(!narrative.Emit("intro",CampusVoice.Inner,"重复开场"),"intro repeated after reload");
        Pass("actual reload "+names[index]+"; position/checkpoint, doors, ride permission, progress and narrative");
    }
}

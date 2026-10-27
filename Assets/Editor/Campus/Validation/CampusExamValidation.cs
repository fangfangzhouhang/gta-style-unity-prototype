using System;
using UnityEngine;

public static class CampusExamValidation
{
    static void Check(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
    public static void CheckSaveRestore(CampusFlowController flow)
    {
        var chapter=UnityEngine.Object.FindObjectOfType<CampusExamChapter>();
        Check(chapter.State.key && chapter.SideOpen,"201 completion missing");
        SaveSystem.Save();
        chapter.State.sideOpen=false;chapter.Restore();
        Check(chapter.sideDoor.GetComponent<Collider>().enabled,"201 restore did not close door");
        Check(SaveSystem.LoadAndApply(false),"201 saved JSON load failed");
        Check(chapter.State.key && chapter.SideOpen && !chapter.sideDoor.GetComponent<Collider>().enabled && !chapter.sideDoor.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled,"201 restored door state incorrect");
        flow.SendMessage("BeginForcedStudy");
        Check(chapter.State.key && chapter.SideOpen,"201 capture lost key progress");
        flow.SendMessage("FinishStudy");
        Debug.Log("CAMPUS_201_SAVE_CAPTURE_PASS");
    }
    public static void CheckRuntime(CampusFlowController flow)
    {
        var chapter=UnityEngine.Object.FindObjectOfType<CampusExamChapter>();
        Check(chapter!=null && !chapter.SideOpen && !chapter.State.key,"201 initial lock invalid");
        var body=flow.player.GetComponent<CharacterController>();
        body.enabled=false;flow.player.position=new Vector3(-10.8f,4.92f,-1.2f);body.enabled=true;Physics.SyncTransforms();
        body.Move(Vector3.left*4);Check(flow.player.position.x> -11.8f,"201 side door bypass");
        body.enabled=false;flow.player.position=new Vector3(-7.5f,4.92f,7);body.enabled=true;Physics.SyncTransforms();
        body.Move(Vector3.forward*4);Check(flow.player.position.z<7.8f,"201 front door bypass");
        foreach(float z in new[]{-2.3f,.55f,3.45f,6.3f})
        {
            body.enabled=false;flow.player.position=new Vector3(-3.7f,4.92f,z);body.enabled=true;Physics.SyncTransforms();body.Move(Vector3.right*4);
            Check(flow.player.position.x< -3.15f,"201 window bypass z="+z);
        }
        Check(!GameObject.Find("VentEntry").GetComponent<ZoneTeleporter>().enabled,"201 vent remains active");
        Check(!chapter.SubmitCode("416"),"remote keypad submission accepted");
        var sample=new SaveData{examChapter=new CampusExamState{paper=true,chart=true,key=true,sideOpen=true,introduction=true}};
        var restored=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(sample));
        Check(restored.examChapter.key && restored.examChapter.sideOpen && restored.examChapter.introduction,"201 save serialization failed");
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);
        Debug.Log("CAMPUS_201_LOCK_PASS");
    }
}

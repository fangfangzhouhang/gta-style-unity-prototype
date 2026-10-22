using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public static class CampusDepartureValidation
{
    static readonly Vector3[] route={new Vector3(16,.92f,20),new Vector3(-6,.92f,41),new Vector3(-14,.92f,41),new Vector3(-14,.92f,49),new Vector3(-10.6f,.92f,48.6f),new Vector3(-14,.92f,50),new Vector3(-14,.92f,55),new Vector3(-20.9f,.92f,59.5f)};
    static int node;
    static float ridingAt;
    static bool tested, mounted, finished;
    static void Check(bool valid,string message){if(!valid)throw new InvalidOperationException(message);}
    public static void CheckInitial(CampusFlowController flow)
    {
        var c=UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>();
        var body=flow.player.GetComponent<CharacterController>();
        foreach(float x in new[]{-18.8f,-17.1f,-16.45f,-14f,-11.55f,-10.8f,-9.2f})
        {body.enabled=false;flow.player.position=new Vector3(x,.92f,50.5f);body.enabled=true;Physics.SyncTransforms();body.Move(Vector3.forward*5);Check(flow.player.position.z<52.6f,"locked gate side bypass x="+x);}
        Check(!c.State.gateOpen && !c.State.departed && !c.State.completed,"departure initially unlocked");
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);Debug.Log("CAMPUS_GATE_INITIAL_LOCK_PASS");
    }
    public static bool Advance(CampusFlowController flow,float dt)
    {
        if(finished)return true;
        var chapter=UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>();var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
        var bike=UnityEngine.Object.FindObjectOfType<CampusMotorcycle>();var motor=flow.player.GetComponent<ThirdPersonController>();
        if(node<route.Length)
        {
            Vector3 delta=route[node]-flow.player.position;delta.y=0;
            if(delta.magnitude>.18f){flow.player.GetComponent<CharacterController>().Move(delta.normalized*Mathf.Min(delta.magnitude,6.2f*dt)+Vector3.down*.35f);return false;}
            Check(flow.player.position.y<1.4f,"departure floor mismatch "+node+" "+flow.player.position);
            if(node==3){flow.NotifySchoolEscaped();Check(!chapter.State.departed && !bike.RideUnlocked,"departed on inside of locked gate");}
            if(node==4 && !tested)
            {
                var control=GameObject.Find("GateRelease").GetComponent<CampusEscapeObject>();motor.controlEnabled=true;
                object[] states={GameProgress.Ensure().examChapter,GameProgress.Ensure().corridorChapter,GameProgress.Ensure().libraryChapter};
                string[][] fields={new[]{"paper","chart","key","sideOpen"},new[]{"card","roomOpen","record","circuit"},new[]{"indexRead","recordRead","manual","key","utilityOpen","emergency"}};
                for(int group=0;group<states.Length;group++)foreach(string name in fields[group])
                {var field=states[group].GetType().GetField(name);field.SetValue(states[group],false);Check(director.TryUse(control)&&!chapter.State.gateOpen,"gate missing prerequisite accepted: "+name);field.SetValue(states[group],true);}
                Check(director.TryUse(control)&&chapter.State.gateOpen && director.GateOpen,"valid gate interaction failed");
                Check(!chapter.gate.GetComponent<Collider>().enabled && !chapter.gate.GetComponent<NavMeshObstacle>().enabled,"gate collision/nav still locked");
                Check(!director.TryUse(control),"open console should no longer consume E");
                SaveSystem.Save();chapter.State.gateOpen=false;chapter.Restore();Check(chapter.gate.GetComponent<Collider>().enabled,"gate relock restore failed");
                Check(SaveSystem.LoadAndApply(false)&&director.GateOpen&&!chapter.gate.GetComponent<Collider>().enabled,"open gate save restore failed");
                Check(Mathf.Abs(chapter.gate.position.x+9)<.01f && chapter.gate.GetComponent<Renderer>().enabled,"open gate visual restore failed");
                motor.controlEnabled=false;tested=true;
            }
            if(node==6){flow.NotifySchoolEscaped();Check(chapter.State.departed && flow.HasEscapedSchool && bike.RideUnlocked && GameProgress.Ensure().escapedSchool,"outside crossing did not unlock ride");}
            Debug.Log("CAMPUS_DEPARTURE_NODE "+node+" "+flow.player.position);node++;return false;
        }
        if(!mounted)
        {
            Check(!chapter.State.completed,"walking completed chapter");
            Check(bike.TryMount(),"cannot mount after physical departure "+flow.player.position+" "+bike.transform.position);
            bike.UseExternalInput=true;mounted=true;ridingAt=Time.time;
        }
        if(!chapter.State.completed)
        {
            Check(Time.time-ridingAt<30,"riding departure timed out "+bike.transform.position);
            Vector3 target=new Vector3(-8,0,86)-bike.transform.position;target.y=0;
            float turn=Vector3.SignedAngle(bike.transform.forward,target,Vector3.up);
            bike.SetDriveInput(1,Mathf.Clamp(turn/25,-1,1),false);return false;
        }
        bike.SetDriveInput(0,0,true);
        if(bike.SpeedKmh>2)return false;
        Check(bike.TryDismount(),"cannot dismount at chapter end");bike.UseExternalInput=false;
        var nav=new NavMeshPath();Check(NavMesh.CalculatePath(new Vector3(-14,0,50),new Vector3(-14,0,57),NavMesh.AllAreas,nav)&&nav.status==NavMeshPathStatus.PathComplete,"open gate navigation incomplete");
        SaveSystem.Save();chapter.State.completed=false;chapter.State.departed=false;chapter.State.gateOpen=false;chapter.Restore();
        Check(!bike.RideUnlocked && !director.GateOpen,"departure reset did not relock");
        Check(SaveSystem.LoadAndApply(false)&&chapter.State.completed&&chapter.State.departed&&director.GateOpen&&bike.RideUnlocked&&flow.HasEscapedSchool,"departure save/load lost flags");
        File.AppendAllText("../CampusValidation/result.txt","\nPASS: all 14 gate prerequisites rejected individually; real gate-console interaction, saved collider/navigation/visual state; outside crossing unlocks original motorcycle; actual physics ride completes chapter; departure flags restore.");
        CampusSceneValidation.RenderDepartureGate();
        Debug.Log("CAMPUS_GATE_MOTORCYCLE_RUN_PASS ridingSeconds="+(Time.time-ridingAt).ToString("F2"));finished=true;return true;
    }
}

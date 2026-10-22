using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public static class CampusLibraryValidation
{
    struct Step
    {public Vector3 feet;public string item;public Step(float x,float y,float z,string name=null){feet=new Vector3(x,y,z);item=name;}}
    static readonly Step[] route={
        new Step(39,8,10),new Step(27,8,9.5f),new Step(27,8,6.6f),new Step(25,8,5.8f,"LibraryIndex"),
        new Step(24.5f,8,4.9f,"LibraryOldRecord"),new Step(24.5f,8,6.2f),new Step(39,8,6.2f),
        new Step(38.5f,8,-1.4f,"Archive_0"),new Step(40.5f,8,-1.4f,"Archive_1"),
        new Step(27,8,-2.5f),new Step(27,8,-5.8f),new Step(15,8,-5.8f),new Step(15,8,6.5f),new Step(15,8,9.5f),
        new Step(45,8,11),new Step(52,8,11),new Step(52,4.2f,.2f),new Step(52,4,-1),new Step(48,4,-1),new Step(48,4,.05f),
        new Step(48,0,11),new Step(45.4f,0,11.4f),new Step(3,0,11.4f),new Step(3,0,9.4f,"LockedUtilityDoor"),
        new Step(3,0,4.9f,"PowerSwitch"),new Step(3,0,9.5f),new Step(3,0,11.3f),new Step(16,0,11.3f),new Step(16,0,14)};
    static int index;
    static bool done;
    static void Check(bool pass,string error){if(!pass)throw new InvalidOperationException(error);}
    static bool Path(Vector3 a,Vector3 b){var p=new NavMeshPath();return NavMesh.CalculatePath(a,b,NavMesh.AllAreas,p)&&p.status==NavMeshPathStatus.PathComplete;}
    static void Probe(CampusFlowController flow,Vector3 at,Vector3 move,Func<Vector3,bool> expect,string label)
    {var body=flow.player.GetComponent<CharacterController>();body.enabled=false;flow.player.position=at;body.enabled=true;Physics.SyncTransforms();body.Move(move);Check(expect(flow.player.position),label+" "+flow.player.position);}
    public static void CheckInitial(CampusFlowController flow)
    {
        var c=UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>();Check(c!=null && !c.State.manual && !c.State.emergency,"library initial state");
        Probe(flow,new Vector3(16,.92f,11.2f),Vector3.forward*4,p=>p.z<12.3f,"yard unlocked before emergency");
        Probe(flow,new Vector3(-14.4f,.92f,31),Vector3.forward*4,p=>p.z<32.2f,"west yard bypass");
        Probe(flow,new Vector3(3,.92f,9.4f),Vector3.back*4,p=>p.z>8.3f,"utility unlocked before key/manual");
        Check(!Path(new Vector3(39,8,10),new Vector3(3,0,10)),"early library-to-ground route");
        foreach(var light in c.emergencyLights)Check(light.intensity==0,"emergency light initially on");
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);Debug.Log("CAMPUS_LIBRARY_INITIAL_LOCK_PASS");
    }
    public static bool Advance(CampusFlowController flow,float dt)
    {
        if(done)return true;
        var c=UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>();var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
        if(index==route.Length)
        {
            Check(c.State.manual && c.State.key && c.State.utilityOpen && c.State.emergency,"library/service facts missing");
            Check(flow.player.position.y<1.3f && flow.player.position.z>13.5f,"not in yard");
            Check(Path(new Vector3(39,8,10),new Vector3(3,0,10)),"unlocked downstairs path incomplete");
            Check(Path(new Vector3(-14.4f,4,18),new Vector3(-14.4f,0,31)),"west shortcut stairs incomplete");
            foreach(var light in c.emergencyLights)Check(light.intensity>1,"emergency light not active");
            CampusSceneValidation.RenderServiceEmergency();
            flow.NotifySchoolEscaped();Check(!flow.HasEscapedSchool && !director.GateOpen,"campus escaped before gate phase");
            SaveSystem.Save();GameProgress.Ensure().libraryChapter=new CampusLibraryState();c.Restore();
            Check(c.utilityDoor.GetComponent<Collider>().enabled && c.yardDoor.GetComponent<Collider>().enabled,"service reset locks failed");
            Check(SaveSystem.LoadAndApply(false) && c.State.emergency && !c.yardDoor.GetComponent<Collider>().enabled && !c.yardDoor.GetComponent<NavMeshObstacle>().enabled,"service saved state restore failed");
            CampusExamValidation.CheckSaveRestore(flow);Check(c.State.manual && c.State.key && c.State.emergency,"capture lost library progress");
            File.AppendAllText("../CampusValidation/result.txt","\nPASS: real library index/book/archive puzzle; rear-gallery return loop; east descent; joint key/manual utility gate; emergency lights and teacher response; physical yard exit; save/load and capture persistence.");
            Debug.Log("CAMPUS_LIBRARY_SERVICE_RUN_PASS");done=true;return true;
        }
        var step=route[index];var delta=step.feet-flow.player.position;delta.y=0;
        if(delta.magnitude>.18f){flow.player.GetComponent<CharacterController>().Move(delta.normalized*Mathf.Min(delta.magnitude,6.2f*dt)+Vector3.down*.35f);return false;}
        Check(Mathf.Abs(flow.player.position.y-step.feet.y-.92f)<.36f,"library route floor mismatch node="+index+" "+flow.player.position);
        if(step.item!=null)
        {
            var item=GameObject.Find(step.item).GetComponent<CampusEscapeObject>();var motor=flow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=true;
            if(step.item=="Archive_1")
            {
                c.State.indexRead=false;Check(director.TryUse(item)&&!c.State.manual,"archive without index");c.State.indexRead=true;
                c.State.recordRead=false;Check(director.TryUse(item)&&!c.State.key,"archive without record");c.State.recordRead=true;
            }
            if(step.item=="LockedUtilityDoor")
            {
                c.State.key=false;Check(director.TryUse(item)&&!c.State.utilityOpen,"utility without key");c.State.key=true;
                c.State.manual=false;Check(director.TryUse(item)&&!c.State.utilityOpen,"utility without manual");c.State.manual=true;
            }
            Check(director.TryUse(item),"library interaction failed: "+step.item+" "+flow.player.position);
            if(step.item=="Archive_0")Check(!c.State.manual && !c.State.key,"wrong archive granted reward");
            if(step.item=="Archive_1")Check(c.State.manual && c.State.key && c.State.downOpen,"correct archive did not unlock");
            if(step.item=="PowerSwitch")
            {
                Check(c.State.emergency,"power did not enable emergency");
                var teacher=GameObject.Find("ServicePatrol").GetComponent<TeacherAgent>();
                Check(teacher.CurrentState==TeacherAgent.State.Investigate || teacher.CurrentState==TeacherAgent.State.Chase,"service teacher ignored alarm");
                Check(!director.TryUse(item)&&c.State.emergency,"completed power interaction was not consumed");
            }
            motor.controlEnabled=false;
        }
        Debug.Log("CAMPUS_LIBRARY_NODE "+index+" "+flow.player.position);index++;return false;
    }
}

using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public static class CampusCorridorValidation
{
    static int index;
    static bool complete;
    static readonly Vector3[] route={
        new Vector3(-15.7f,4,-1.2f),new Vector3(-15.7f,4,9),new Vector3(-7,4,9.8f),
        new Vector3(-3,4,10.3f),new Vector3(15,4,9.4f),new Vector3(12,4,10.4f),new Vector3(15,4,9.4f),new Vector3(15,4,6.7f),new Vector3(13,4,4.8f),
        new Vector3(15,4,-.5f),new Vector3(13,4,-3),new Vector3(15,4,-3),new Vector3(17,4,-3),new Vector3(15,4,-.5f),
        new Vector3(15,4,9.5f),new Vector3(23,4,9.2f),new Vector3(23.75f,4,9.2f),new Vector3(23.75f,4,11.5f),new Vector3(45.4f,4,11.5f),
        new Vector3(45.4f,4,-1),new Vector3(52,4,-1),new Vector3(52,4,.2f),new Vector3(52,8,10.9f),new Vector3(45,8,11)};
    static void Check(bool ok,string error){if(!ok)throw new InvalidOperationException(error);}
    static bool Path(Vector3 a,Vector3 b){var path=new NavMeshPath();return NavMesh.CalculatePath(a,b,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;}
    public static void CheckInitial(CampusFlowController flow)
    {
        var c=UnityEngine.Object.FindObjectOfType<CampusCorridorChapter>();Check(c!=null && !c.State.card && !c.State.circuit,"corridor initial state");
        var npcs=UnityEngine.Object.FindObjectsOfType<CampusCorridorNPC>();
        Check(npcs.Length==3,"three corridor NPCs required");
        foreach(var npc in npcs)Check(npc.GetComponent<NavMeshAgent>().isOnNavMesh,"NPC not on gallery NavMesh");
        Check(!Path(new Vector3(15,4,10),new Vector3(15,8,10)),"locked upper stairs bypassed");
        Check(!Path(new Vector3(15,4,10),new Vector3(48,0,11)),"locked ground route bypassed");
        foreach(var pane in GameObject.Find("ChapterCorridorEnclosure").GetComponentsInChildren<Renderer>())Check(pane.sharedMaterial!=null,"security glazing material missing after scene reload");
        Check(GameObject.Find("AlarmCircuit").GetComponent<Renderer>().sharedMaterial.color.g>.5f && GameObject.Find("LightingCircuit").GetComponent<Renderer>().sharedMaterial.color.g<.3f,"initial switch colors do not match saved state");
        var body=flow.player.GetComponent<CharacterController>();
        Sweep(body,flow.player,new Vector3(15,4.92f,9.4f),Vector3.back*4,p=>p.z>8.3f,"203 card door");
        Sweep(body,flow.player,new Vector3(15,4.92f,-2.8f),Vector3.back*4,p=>p.z> -4.1f,"203 rear exit seal");
        Sweep(body,flow.player,new Vector3(-14.4f,4.92f,19),Vector3.forward*4,p=>p.z<20.3f,"west stairs");
        Sweep(body,flow.player,new Vector3(43,4.92f,10),Vector3.right*4,p=>p.x<44.2f,"east gate");
        Sweep(body,flow.player,new Vector3(22,5.7f,11.7f),Vector3.forward*4,p=>p.z<12.3f,"gallery jump guard");
        var motor=flow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=true;
        body.enabled=false;flow.player.position=new Vector3(15,4.92f,9.4f);body.enabled=true;Physics.SyncTransforms();
        var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
        director.TryUse(GameObject.Find("Equipment203Door").GetComponent<CampusEscapeObject>());
        Check(!c.State.roomOpen,"203 opened without card");motor.controlEnabled=false;
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);Debug.Log("CAMPUS_CORRIDOR_LOCK_PASS");
    }
    static void Sweep(CharacterController body,Transform player,Vector3 at,Vector3 move,Func<Vector3,bool> verify,string label)
    {body.enabled=false;player.position=at;body.enabled=true;Physics.SyncTransforms();body.Move(move);Check(verify(player.position),"physical lock failed: "+label+" "+player.position);}
    public static bool Advance(CampusFlowController flow,float dt)
    {
        if(complete)return true;
        var c=UnityEngine.Object.FindObjectOfType<CampusCorridorChapter>();
        if(index==route.Length)
        {
            Check(flow.HasEscapedExam && GameProgress.Ensure().stage==GameProgress.StageSchool,"201 exit not recorded after crossing door");
            Check(c.State.conversation && c.State.notice && c.State.card && c.State.roomOpen && c.State.record && c.State.circuit,"corridor route facts missing");
            Check(flow.player.position.y>8.8f,"not on third floor");
            Check(Path(new Vector3(15,4,10),new Vector3(15,8,10)),"unlocked upper stairs disconnected");
            Check(!Path(new Vector3(15,4,10),new Vector3(48,0,11)),"ground floor opened too early");
            var body=flow.player.GetComponent<CharacterController>();var before=flow.player.position;
            Sweep(body,flow.player,new Vector3(52.3f,4.92f,-1),Vector3.right*4,p=>p.x<53.5f,"middle landing outer edge");
            Sweep(body,flow.player,new Vector3(47.5f,8.92f,11),Vector3.back*4,p=>p.z>10.1f,"third-floor landing rear edge");
            Sweep(body,flow.player,new Vector3(48,4.92f,-1),Vector3.forward*4,p=>p.z<-.2f,"downstairs still locked after circuit");
            body.enabled=false;flow.player.position=before;body.enabled=true;Physics.SyncTransforms();
            SaveSystem.Save();GameProgress.Ensure().corridorChapter=new CampusCorridorState();c.Restore();
            Check(c.equipmentDoor.GetComponent<Collider>().enabled && c.eastGate.GetComponent<Collider>().enabled,"reset gates did not close");
            Check(SaveSystem.LoadAndApply(false) && c.State.circuit && c.State.card && !c.eastGate.GetComponent<Collider>().enabled && !c.eastGate.GetComponent<NavMeshObstacle>().enabled,"corridor save restore failed");
            CampusExamValidation.CheckSaveRestore(flow);Check(c.State.card && c.State.circuit,"capture lost corridor progress");
            File.AppendAllText("../CampusValidation/result.txt","\nPASS: physical 201 -> repeatable NPC/notice -> access card -> 203 -> three switches -> east stairs -> third floor; ground remains locked; save/load and capture retain stages A-C.");
            body.enabled=false;flow.player.position=before;body.enabled=true;Physics.SyncTransforms();
            flow.player.GetComponent<ThirdPersonController>().controlEnabled=false;
            Debug.Log("CAMPUS_CORRIDOR_RUN_PASS");complete=true;return true;
        }
        Vector3 delta=route[index]-flow.player.position;delta.y=0;
        if(delta.magnitude>.2f){flow.player.GetComponent<CharacterController>().Move(delta.normalized*Mathf.Min(delta.magnitude,6.2f*dt)+Vector3.down*.35f);return false;}
        Check(Mathf.Abs(flow.player.position.y-(route[index].y+.92f))<.35f,"wrong route floor node="+index+" "+flow.player.position);
        string[] names={null,null,"Student_Lin","CorridorNotice","Equipment203Door","AccessCardLocker","Equipment203Door",null,"EquipmentRecord","CircuitConfirm","LightingCircuit","AlarmCircuit","DoorPowerCircuit","CircuitConfirm",null,"DiversionBell"};
        if(index<names.Length && names[index]!=null)
        {
            var item=GameObject.Find(names[index]).GetComponent<CampusEscapeObject>();
            var motor=flow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=true;
            Check(UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>().TryUse(item),"corridor interaction failed "+names[index]+" "+flow.player.position);
            if(index==2)Check(UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>().TryUse(item),"NPC clue cannot repeat");
            if(index==4)Check(!c.State.roomOpen,"203 opened without card after leaving 201");
            if(index==9)
            {
                // The initial switch pattern must fail even with the record discovered.
                Check(!c.State.circuit,"incorrect circuit accepted");
            }
            if(index==13)Check(c.State.circuit,"correct circuit rejected");
            motor.controlEnabled=false;
        }
        Debug.Log("CAMPUS_CORRIDOR_NODE "+index+" "+flow.player.position);index++;return false;
    }
}

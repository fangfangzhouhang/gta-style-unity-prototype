using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public static class CampusGameplayValidation
{
    static CampusFlowController runningFlow;
    static int routeIndex;
    static float routeStarted, lastTime;
    static bool completed;
    static int examNode;
    static readonly Vector3[] examRoute={new Vector3(-8.6f,4,-2.2f),new Vector3(-6.6f,4,-2.8f),new Vector3(-10.8f,4,-2),new Vector3(-11,4,-1.2f)};
    static readonly Vector3[] route={new Vector3(-10.7f,4,-2.2f),new Vector3(-11,4,-1.2f),
        new Vector3(-15.7f,4,-1.2f),new Vector3(-15.7f,4,9),new Vector3(15,4,9),new Vector3(15,4,5.3f),
        new Vector3(15,4,9),new Vector3(23,4,9),new Vector3(45.4f,4,9),new Vector3(45.4f,4,-1),
        new Vector3(48,4,-1),new Vector3(48,4,.05f),new Vector3(48,0,11),new Vector3(48,0,14),
        new Vector3(3,0,14),new Vector3(3,0,9.4f),new Vector3(3,0,4.9f),new Vector3(3,0,10),
        new Vector3(3,0,41.5f),new Vector3(-16.5f,0,41.5f),new Vector3(-16.5f,0,48),
        new Vector3(-11.6f,0,49.5f),new Vector3(-14.4f,0,54)};
    static void Require(bool pass,string error) { if(!pass) throw new InvalidOperationException(error); }
    public static void Run(CampusFlowController flow)
    {
        var motor=flow.player.GetComponent<ThirdPersonController>();
        var body=flow.player.GetComponent<CharacterController>();
        Require(motor.sprintSpeed>=6f && motor.walkSpeed>=2.8f,"主角速度未更新");
        foreach(var teacher in UnityEngine.Object.FindObjectsOfType<TeacherAgent>())
        {
            var nav=teacher.GetComponent<NavMeshAgent>();
            Require(nav!=null && nav.enabled && nav.isOnNavMesh,"抓捕者未接入导航: "+teacher.name);
            Require(teacher.chaseSpeed<motor.sprintSpeed && teacher.catchDistance<0.9f,"抓捕平衡错误");
            teacher.SetPaused(true);
        }
        var path=new NavMeshPath();
        Require(UnityEngine.Object.FindObjectOfType<CampusCorridorChapter>()!=null || NavMesh.CalculatePath(new Vector3(-14.4f,4,-1.2f),new Vector3(-14.4f,0,50),NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,
            "二楼到校门导航路径不连通");
        motor.controlEnabled=false;
        body.enabled=false; flow.player.position=new Vector3(-3.7f,4.92f,3.45f); body.enabled=true;
        Physics.SyncTransforms();
        body.Move(Vector3.right*8f);
        Require(flow.player.position.x<-3.15f,"角色穿过玻璃窗碰撞体");
        flow.SetZone(CampusFlowController.Zone.School);
        flow.activeCheckpoint=GameObject.Find(UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>()!=null?"EastGroundCheckpoint":"GroundSchoolCheckpoint").transform;
        Vector3 expected=flow.activeCheckpoint.position;
        body.enabled=false; flow.player.position=new Vector3(-14.4f,-10f,33f); body.enabled=true;
        flow.player.GetComponent<CampusSafety>().SendMessage("LateUpdate");
        Require(Vector3.Distance(flow.player.position,expected)<0.25f,"坠落后未返回最近检查点: "+flow.player.position);
        Require(motor.controlEnabled && SafeDiscoveryReset.IsInvulnerable,"回位后控制或保护期错误");
        Require(GameObject.Find("TownGround").GetComponent<Renderer>().bounds.size.x>=180f,"地图没有扩建");
        Require(UnityEngine.Object.FindObjectsOfType<LODGroup>().Length>=30,"树木细节层级缺失");
        foreach(var light in UnityEngine.Object.FindObjectsOfType<Light>())
        {
            if(light.name=="InteriorLight") Require(light.color.b>light.color.r,"灯管必须是冷白光");
            if(light.name=="ScrutinyLight") Require(light.type==LightType.Spot,"抓捕者必须使用定向警戒光");
        }
        CampusSiteValidation.CheckRiding(flow);
        CampusEscapeValidation.CheckRuntime(flow);
        CampusExamValidation.CheckRuntime(flow);
        if(UnityEngine.Object.FindObjectOfType<CampusCorridorChapter>()!=null)CampusCorridorValidation.CheckInitial(flow);
        if(UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>()!=null)CampusLibraryValidation.CheckInitial(flow);
        if(UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>()!=null)CampusDepartureValidation.CheckInitial(flow);
        flow.SetZone(CampusFlowController.Zone.Exam);
        flow.activeCheckpoint=flow.examCheckpoint;
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);
        foreach(var teacher in UnityEngine.Object.FindObjectsOfType<TeacherAgent>()) teacher.SetPaused(false);
        runningFlow=flow;
        routeStarted=Time.time;
        lastTime=Time.time;
        motor.controlEnabled=false;
        File.AppendAllText("../CampusValidation/result.txt","\nPASS: actor NavMesh; 2F-to-gate path; swept player/window collision; falling recovery to ground checkpoint; speed/light/map/foliage balance.");
        Debug.Log("CAMPUS_GAMEPLAY_VALIDATION_PASS");
    }

    public static bool AdvanceEscape()
    {
        if(completed) return true;
        if(runningFlow==null) return false;
        Require(!runningFlow.IsBusy,"自动逃脱途中被抓住 node="+routeIndex+" at="+runningFlow.player.position);
        Require(Time.time-routeStarted<120f,"自动逃脱超时 node="+routeIndex+" at="+runningFlow.player.position);
        if(runningFlow.HasEscapedSchool && UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>()==null)
        {
            var completedEscape=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
            Require(completedEscape.PlanRead && completedEscape.HasKey && completedEscape.UtilityOpen && completedEscape.PowerOff && completedEscape.GateOpen,"过关时谜题阶段缺失");
            CampusExamValidation.CheckSaveRestore(runningFlow);
            completed=true;
            runningFlow.player.GetComponent<ThirdPersonController>().controlEnabled=true;
            File.AppendAllText("../CampusValidation/result.txt","\nPASS: live CharacterController route with plan, key, utility door, power and gate interactions; east stairs and patrols; escape without capture. Seconds="+(Time.time-routeStarted).ToString("F2"));
            Debug.Log("CAMPUS_ESCAPE_RUN_PASS");
            return true;
        }
        Require(routeIndex<route.Length,"已到校门但未触发逃脱流程");
        float dt=Mathf.Min(Time.time-lastTime,0.05f);
        lastTime=Time.time;
        if(dt<=0) return false;
        if(examNode<examRoute.Length)
        {
            Vector3 move=examRoute[examNode]-runningFlow.player.position;move.y=0;
            if(move.magnitude>.15f){runningFlow.player.GetComponent<CharacterController>().Move(move.normalized*Mathf.Min(move.magnitude,6.2f*dt)+Vector3.down*.35f);return false;}
            var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
            var motor=runningFlow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=true;
            string[] names={"ChapterPaper","ChapterSeatChart","ChapterCabinet","Chapter201SideDoor"};
            Require(director.TryUse(GameObject.Find(names[examNode]).GetComponent<CampusEscapeObject>()),"201 interaction failed node="+examNode);
            if(examNode==2)
            {
                var chapter=UnityEngine.Object.FindObjectOfType<CampusExamChapter>();
                var camera=UnityEngine.Object.FindObjectOfType<ThirdPersonCamera>();
                Require(UnityEngine.Object.FindObjectOfType<CampusJournal>()==null || !UnityEngine.Object.FindObjectOfType<CampusJournal>().Open(),"journal opened over keypad");
                Require(chapter.KeypadOpen && Cursor.lockState==CursorLockMode.None && Cursor.visible,"keypad did not release cursor");
                Require(!camera.TryCapturePointerOnClick() && Cursor.lockState==CursorLockMode.None && Cursor.visible,"camera stole keypad click");
                Require(!chapter.SubmitCode("000"),"wrong code accepted");
                Require(chapter.KeypadOpen && !camera.TryCapturePointerOnClick(),"wrong code released modal pointer ownership");
                Require(chapter.SubmitCode("416"),"201 correct code rejected");
                Require(!chapter.KeypadOpen && camera.TryCapturePointerOnClick(),"camera control not restored after keypad closed");
                Debug.Log("CAMPUS_KEYPAD_POINTER_PASS");
            }
            Debug.Log("CAMPUS_201_NODE "+examNode);motor.controlEnabled=false;examNode++;return false;
        }
        if(UnityEngine.Object.FindObjectOfType<CampusCorridorChapter>()!=null)
        {
            if(!CampusCorridorValidation.Advance(runningFlow,dt))return false;
            if(UnityEngine.Object.FindObjectOfType<CampusLibraryChapter>()!=null && !CampusLibraryValidation.Advance(runningFlow,dt))return false;
            if(UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>()!=null && !CampusDepartureValidation.Advance(runningFlow,dt))return false;
            File.AppendAllText("../CampusValidation/result.txt","\nPASS: staged chapter route, Seconds="+(Time.time-routeStarted).ToString("F2"));
            Debug.Log("CAMPUS_STAGED_CHAPTER_COMPLETE Seconds="+(Time.time-routeStarted).ToString("F2"));
            completed=true;runningFlow.player.GetComponent<ThirdPersonController>().controlEnabled=true;return true;
        }
        Vector3 delta=route[routeIndex]-runningFlow.player.position; delta.y=0;
        if(delta.magnitude<0.22f)
        {
            string item=routeIndex==0?"EvacuationPlan":routeIndex==5?"MaintenanceKey":routeIndex==7?"DiversionBell":routeIndex==15?"LockedUtilityDoor":routeIndex==16?"PowerSwitch":routeIndex==21?"GateRelease":null;
            if(item!=null)
            {
                var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
                var motor=runningFlow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=true;
                bool used=director.TryUse(GameObject.Find(item).GetComponent<CampusEscapeObject>());motor.controlEnabled=false;
                Require(used,"路线交互失败: "+item+" player="+runningFlow.player.position);
            }
            Debug.Log("CAMPUS_ROUTE_NODE "+routeIndex+" "+runningFlow.player.position);routeIndex++;return false;
        }
        runningFlow.player.GetComponent<CharacterController>().Move(delta.normalized*Mathf.Min(delta.magnitude,6.2f*dt)+Vector3.down*0.35f);
        return false;
    }
}

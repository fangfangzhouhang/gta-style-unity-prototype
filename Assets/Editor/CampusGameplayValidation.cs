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
    static readonly Vector3[] route={new Vector3(-11,0,-2.2f),new Vector3(-11,0,-1.2f),
        new Vector3(-15.7f,0,-1.2f),new Vector3(-15.7f,0,21.4f),new Vector3(-14.4f,0,21.8f),
        new Vector3(-14.4f,0,32.5f),new Vector3(-16.5f,0,34),new Vector3(-16.5f,0,50),new Vector3(-14.4f,0,54)};
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
        Require(NavMesh.CalculatePath(new Vector3(-14.4f,4,-1.2f),new Vector3(-14.4f,0,50),NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,
            "二楼到校门导航路径不连通");
        motor.controlEnabled=false;
        body.enabled=false; flow.player.position=new Vector3(-3.7f,4.92f,3.45f); body.enabled=true;
        Physics.SyncTransforms();
        body.Move(Vector3.right*8f);
        Require(flow.player.position.x<-3.15f,"角色穿过玻璃窗碰撞体");
        flow.SetZone(CampusFlowController.Zone.School);
        flow.activeCheckpoint=GameObject.Find("GroundSchoolCheckpoint").transform;
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
        Require(!runningFlow.IsBusy,"自动逃脱途中被抓住，路线平衡需要调整");
        Require(Time.time-routeStarted<60f,"自动逃脱超时，可能被碰撞体卡住");
        if(runningFlow.HasEscapedSchool)
        {
            completed=true;
            runningFlow.player.GetComponent<ThirdPersonController>().controlEnabled=true;
            File.AppendAllText("../CampusValidation/result.txt","\nPASS: live CharacterController sprint from exam through patrolled corridors/stairs to school gate without capture. Seconds="+(Time.time-routeStarted).ToString("F2"));
            Debug.Log("CAMPUS_ESCAPE_RUN_PASS");
            return true;
        }
        Require(routeIndex<route.Length,"已到校门但未触发逃脱流程");
        float dt=Mathf.Min(Time.time-lastTime,0.05f);
        lastTime=Time.time;
        if(dt<=0) return false;
        Vector3 delta=route[routeIndex]-runningFlow.player.position; delta.y=0;
        if(delta.magnitude<0.22f) {routeIndex++;return false;}
        runningFlow.player.GetComponent<CharacterController>().Move(delta.normalized*Mathf.Min(delta.magnitude,6.2f*dt)+Vector3.down*0.35f);
        return false;
    }
}

using System;
using UnityEngine;
using UnityEngine.AI;

public static class CampusEscapeValidation
{
    static void Require(bool valid,string message){if(!valid)throw new InvalidOperationException(message);}
    public static void CheckLayout()
    {
        var building=GameObject.Find("TeachingBuilding_OpenFloors");
        Require(building!=null && GameObject.Find("ExamHall").transform.IsChildOf(building.transform),"201 没有整合进教学楼");
        for(int level=0;level<3;level++)
        {
            FloorAt(new Vector3(15,level*4,10));FloorAt(new Vector3(15,level*4,-6));
            FloorAt(new Vector3(15,level*4,-18));FloorAt(new Vector3(-10,level*4,-12));FloorAt(new Vector3(42,level*4,-12));
        }
        FloorAt(new Vector3(45.4f,4,5));FloorAt(new Vector3(48,4,-1));FloorAt(new Vector3(52,8,11));
        var parcels=UnityEngine.Object.FindObjectsOfType<TownParcel>();
        foreach(var parcel in parcels)
        {
            var bounds=new Bounds(parcel.transform.position+Vector3.up,new Vector3(parcel.footprint.x,2,parcel.footprint.y));
            foreach(var other in parcels)
            {if(parcel==other)continue;var b=new Bounds(other.transform.position+Vector3.up,new Vector3(other.footprint.x,2,other.footprint.y));Require(!bounds.Intersects(b),"城镇地块重叠: "+parcel.parcelId+"/"+other.parcelId);}
            Require(!bounds.Intersects(new Bounds(new Vector3(-28,1,118),new Vector3(9.5f,2,10))),"地块占用现有家庭室内");
        }
        Require(parcels.Length==13,"杭州参考地块数量错误");Debug.Log("CAMPUS_V11_LAYOUT_PASS");
        Require(Physics.Raycast(new Vector3(-81,1.8f,80),Vector3.up,out RaycastHit deck,8,~0,QueryTriggerInteraction.Ignore) && deck.point.y>5.2f,"中河高架路口桥下净空不足");
    }
    static void FloorAt(Vector3 feet)
    {
        Require(Physics.Raycast(feet+Vector3.up*.1f,Vector3.down,out RaycastHit hit,.3f,~0,QueryTriggerInteraction.Ignore),"开放楼层缺地板: "+feet);
        Require(Mathf.Abs(hit.point.y-feet.y)<.05f,"楼层高度错误: "+feet);
        Require(Physics.Raycast(feet+Vector3.up*1.65f,Vector3.up,out RaycastHit roof,3,~0,QueryTriggerInteraction.Ignore),"室内或有顶外廊缺屋顶: "+feet);
        Require(roof.point.y-feet.y>3.3f,"楼层净高不足: "+feet+" roof="+roof.collider.name);
    }
    public static void CheckRuntime(CampusFlowController flow)
    {
        var director=UnityEngine.Object.FindObjectOfType<CampusEscapeDirector>();
        Require(director!=null && !director.PlanRead && !director.HasKey && !director.PowerOff && !director.GateOpen,"新逃生阶段初态错误");
        flow.NotifySchoolEscaped();Require(!flow.HasEscapedSchool,"未解锁校门即可过关");
        var body=flow.player.GetComponent<CharacterController>();
        foreach(float x in new[]{-18f,-10f})
        {
            body.enabled=false;flow.player.position=new Vector3(x,.92f,51);body.enabled=true;Physics.SyncTransforms();body.Move(Vector3.forward*4);
            Require(flow.player.position.z<52,"闭门可从柱子侧面绕过: "+flow.player.position);
        }
        SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint);
        var path=new NavMeshPath();
        Require(NavMesh.CalculatePath(new Vector3(15,4,10),new Vector3(15,8,10),NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"二楼至三楼导航不连通");
        Require(NavMesh.CalculatePath(new Vector3(15,4,10),new Vector3(48,0,11),NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"东楼梯上下楼不连通");
        var teacher=GameObject.Find("Patrol_Gallery").GetComponent<TeacherAgent>();teacher.SetPaused(false);
        Require(teacher.InvestigateNoise(new Vector3(23,5,10),22) && teacher.CurrentState==TeacherAgent.State.Investigate,"老师不调查声源");
        teacher.ReturnHome();teacher.SetPaused(true);Debug.Log("CAMPUS_ESCAPE_PUZZLE_NAV_PASS");
    }
}

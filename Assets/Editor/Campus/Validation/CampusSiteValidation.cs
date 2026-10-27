using System;
using System.IO;
using UnityEngine;

public static class CampusSiteValidation
{
    static void Require(bool valid,string message){if(!valid)throw new InvalidOperationException(message);}
    public static void CheckLayout()
    {
        var terrain=GameObject.Find("TownGround");
        Require(terrain!=null && terrain.GetComponent<MeshCollider>()!=null,"统一地形碰撞缺失");
        Require(GameObject.Find("WestGardenGround")==null && GameObject.Find("BldgA")==null,"旧重叠地面/占位楼没有移除");
        Require(GameObject.Find("GateVisual")==null,"校门不应保留不透明发光平面");
        Require(CampusSitePlan.Height(125,-150)-CampusSitePlan.Height(0,100)>1,"地形起伏缺失");
        for(float x=-22;x<=2;x+=2)for(float z=54;z<=65;z+=2)
        {
            var hits=Physics.RaycastAll(new Vector3(x,8,z),Vector3.down,9,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<hits.Length;i++)for(int j=i+1;j<hits.Length;j++)
                if(hits[i].normal.y>.99f && hits[j].normal.y>.99f && hits[i].point.y<.2f && hits[j].point.y<.2f)
                    Require(Mathf.Abs(hits[i].point.y-hits[j].point.y)>.005f,"校门附近存在同面重叠: "+hits[i].collider.name+" / "+hits[j].collider.name);
        }
        for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
        {
            var a=GameObject.Find("TownHouse_"+i).transform.Find("BuildingBody").GetComponent<Renderer>().bounds;
            var b=GameObject.Find("TownHouse_"+j).transform.Find("BuildingBody").GetComponent<Renderer>().bounds;
            a.Expand(4);Require(!a.Intersects(b),"小房子间距不足");
        }
        Require(GameObject.Find("GateMotorcycle")!=null && GameObject.Find("Library")!=null && GameObject.Find("DiningHall")!=null,"校园设施缺失");
        Debug.Log("CAMPUS_SITE_LAYOUT_PASS");
    }
    public static void CheckRiding(CampusFlowController flow)
    {
        var bike=UnityEngine.Object.FindObjectOfType<CampusMotorcycle>();var rigid=bike.GetComponent<Rigidbody>();
        var cc=flow.player.GetComponent<CharacterController>();
        var savedPosition=rigid.position;var savedRotation=rigid.rotation;
        cc.enabled=false;flow.player.position=savedPosition+Vector3.left*1.4f+Vector3.up*.9f;cc.enabled=true;Physics.SyncTransforms();
        var departure=UnityEngine.Object.FindObjectOfType<CampusDepartureChapter>();
        bool previousDeparture=departure!=null && departure.State.departed;
        if(departure!=null){Require(!bike.TryMount(),"提前骑乘没有封锁");departure.State.departed=true;}
        Require(bike.TryMount(),"摩托车不能上车");
        Require(!cc.enabled && !flow.player.GetComponent<ThirdPersonController>().controlEnabled,"上车没有切换控制");
        var simulationMode=Physics.simulationMode;
        GameObject wall=null;
        try
        {
            Physics.simulationMode=SimulationMode.Script;bike.UseExternalInput=true;
            rigid.position=new Vector3(-8,.12f,135);rigid.rotation=Quaternion.identity;rigid.velocity=Vector3.zero;Physics.SyncTransforms();
            bike.SetDriveInput(1,0,false);
            for(int i=0;i<225;i++){bike.SendMessage("FixedUpdate");Physics.Simulate(.02f);}
            Require(bike.SpeedKmh>40 && bike.SpeedKmh<65,"通勤摩托加速不合理: "+bike.SpeedKmh);
            Require(rigid.position.z>160,"车辆没有实际前进");
            Require(!bike.TryDismount(),"高速时不应允许跳车");
            rigid.velocity=Vector3.forward*35;bike.SendMessage("FixedUpdate");Physics.Simulate(.02f);
            Require(bike.SpeedKmh<=90.1f,"车速没有限制在 90km/h");
            bike.SetDriveInput(0,0,true);
            for(int i=0;i<220;i++){bike.SendMessage("FixedUpdate");Physics.Simulate(.02f);}
            Require(bike.SpeedKmh<1,"车辆不能刹停");
            // Same speed and input: actual Rigidbody yaw must rise by 30%.
            float[] turn=new float[2];
            for(int pass=0;pass<2;pass++)
            {
                rigid.position=new Vector3(-8,.12f,140);rigid.rotation=Quaternion.identity;rigid.velocity=Vector3.forward*8;
                bike.steeringMultiplier=pass==0?1:1.3f;bike.SetDriveInput(0,.5f,false);Physics.SyncTransforms();
                bike.SendMessage("FixedUpdate");Physics.Simulate(.02f);turn[pass]=Mathf.Abs(Mathf.DeltaAngle(0,rigid.rotation.eulerAngles.y));
            }
            Require(turn[0]>.1f && Mathf.Abs(turn[1]/turn[0]-1.3f)<.03f,"摩托车转向增幅不是30%");
            // Force a direct high-speed impact against a real static collider.
            rigid.position=new Vector3(-8,.12f,140);rigid.rotation=Quaternion.identity;rigid.velocity=Vector3.forward*20;
            wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="ValidationBikeWall";wall.transform.position=new Vector3(-8,1.5f,145);wall.transform.localScale=new Vector3(5,3,.3f);Physics.SyncTransforms();
            bike.SetDriveInput(1,0,false);
            for(int i=0;i<70;i++){bike.SendMessage("FixedUpdate");Physics.Simulate(.02f);}
            Require(rigid.position.z<144.4f,"摩托车高速穿墙");
            UnityEngine.Object.DestroyImmediate(wall);wall=null;
            // Park on actual uneven terrain and check exit height is raycast based.
            rigid.position=new Vector3(105,CampusSitePlan.Height(105,135)+.04f,135);
            rigid.velocity=Vector3.zero;bike.SetDriveInput(0,0,true);Physics.SyncTransforms();
            Require(bike.TryDismount(),"停车后不能安全下车");
            Require(cc.enabled && flow.player.GetComponent<ThirdPersonController>().controlEnabled,"下车控制没有恢复");
            Require(flow.player.parent!=bike.transform,"下车仍附着车辆");
            Require(Physics.Raycast(flow.player.position,Vector3.down,out RaycastHit support,1.1f,~0,QueryTriggerInteraction.Ignore) && support.distance>.85f,"起伏地形下车没有贴地");
            Require(bike.TryMount(),"起伏地形不能再次上车");
            Require(SafeDiscoveryReset.TryRespawn(flow.player,flow.examCheckpoint),"骑行时不能返回检查点");
            Require(!bike.IsRiding && cc.enabled && flow.player.parent!=bike.transform,"回出生点仍残留骑行状态");
            Debug.Log("CAMPUS_MOTORCYCLE_PHYSICS_PASS steeringRatio="+(turn[1]/turn[0]).ToString("F3"));
        }
        finally
        {
            if(wall!=null)UnityEngine.Object.DestroyImmediate(wall);
            bike.UseExternalInput=false;Physics.simulationMode=simulationMode;
            if(departure!=null)departure.State.departed=previousDeparture;
            rigid.position=savedPosition;rigid.rotation=savedRotation;rigid.velocity=Vector3.zero;rigid.angularVelocity=Vector3.zero;
        }
    }
}

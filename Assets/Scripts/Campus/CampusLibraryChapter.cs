using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CampusLibraryState
{
    public bool indexRead, recordRead, manual, key, downOpen, utilityOpen, emergency;
}

/// <summary>Library and service-area progression, dispatched by CampusEscapeDirector.</summary>
public class CampusLibraryChapter : MonoBehaviour
{
    public Transform eastDown, westDown, libraryShortcut, returnDoor, yardDoor, westYardDoor, utilityDoor;
    public Light[] emergencyLights;
    CampusExamChapter subtitles;
    readonly Dictionary<Light,float> originalLighting=new Dictionary<Light,float>();
    public CampusLibraryState State { get { return GameProgress.Ensure().libraryChapter; } }
    public string Objective { get { return !State.manual?"在三楼图书馆查找紧急说明与维修钥匙":!State.utilityOpen?"带紧急说明和维修钥匙，进入一楼配电间":!State.emergency?"启动紧急疏散模式":"庭院通道已开放，前往校门"; } }
    public string Detail { get { return !State.manual?"从三楼前廊进入图书馆，结合书目索引和旧检修记录检索档案":!State.utilityOpen?"东西楼梯均已开放；一楼 102 是配电间，钥匙和紧急说明缺一不可":!State.emergency?"配电间内部按 E 启动紧急模式；灯光与广播会变化":"庭院前门与西侧通道可任选一路；校门流程待下一阶段接通"; } }
    public static void Build(Transform root)
    {
        var c=root.gameObject.AddComponent<CampusLibraryChapter>();var props=root.Find("EscapeObjectives");
        var geometry=new GameObject("LibraryAndServiceChapter").transform;geometry.SetParent(root,false);
        foreach(string name in new[]{"Classroom_304","Classroom_305"}){var old=GameObject.Find(name);if(old!=null)LevelBuildUtil.DestroyObject(old);}
        Color wood=new Color(.42f,.26f,.13f), wall=new Color(.84f,.78f,.64f);
        Part(geometry,"LibraryFloor",new Vector3(33,7.9f,2),new Vector3(22,.2f,12),new Color(.48f,.43f,.34f));
        Part(geometry,"LibraryCeiling",new Vector3(33,11.5f,2),new Vector3(22,.2f,12),wall);
        foreach(float x in new[]{22f,44f})Part(geometry,"LibrarySideWall",new Vector3(x,9.7f,2),new Vector3(.18f,3.4f,12),wall);
        // Two front entries and one initially locked rear shortcut, with solid lintels.
        WallSegment(geometry,22,26.1f,8,wall);WallSegment(geometry,27.9f,38.1f,8,wall);WallSegment(geometry,39.9f,44,8,wall);
        foreach(float x in new[]{27f,39f})Part(geometry,"LibraryDoorHead",new Vector3(x,10.9f,8),new Vector3(1.8f,1,.18f),wall);
        WallSegment(geometry,22,26.1f,-4,wall);WallSegment(geometry,27.9f,44,-4,wall);
        Part(geometry,"LibraryBackHead",new Vector3(27,10.9f,-4),new Vector3(1.8f,1,.18f),wall);
        c.libraryShortcut=Gate(props,"LibraryRearShortcut",new Vector3(27,9.2f,-4),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.LibraryShortcut,"档案解锁后可打开回访捷径");
        c.returnDoor=Gate(props,"LibraryReturn303",new Vector3(15,9.2f,8),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.LibraryShortcut,"回访通道尚未解锁");
        foreach(float x in new[]{-7.5f,3f})Gate(props,"ThirdClosedRoom_"+x,new Vector3(x,9.2f,8),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.LibraryBarrier,"非开放区域");
        foreach(Vector2 gap in new[]{new Vector2(-2.5f,1),new Vector2(9,2),new Vector2(21,2)})Part(geometry,"ThirdGapInfill",new Vector3(gap.x,9.7f,8),new Vector3(gap.y,3.4f,.18f),wall);
        foreach(float x in new[]{30f,35f})
        {
            Part(geometry,"LibraryBookcase",new Vector3(x,9.1f,2),new Vector3(.5f,2.2f,5.8f),wood);
            for(int shelf=0;shelf<4;shelf++)for(int book=0;book<8;book++)foreach(float side in new[]{-1f,1f})
                Part(geometry,"BookSpine",new Vector3(x+side*.29f,8.4f+shelf*.5f,-.4f+book*.64f),new Vector3(.12f,.35f,.44f),new Color(.22f+book*.04f,.27f+shelf*.08f,.34f));
        }
        foreach(float x in new[]{26f,39f})
        {
            var go=new GameObject("LibraryReadingLight");go.transform.SetParent(geometry,false);go.transform.position=new Vector3(x,11,2);
            var light=go.AddComponent<Light>();light.type=LightType.Point;light.range=9;light.intensity=.7f;light.color=new Color(.95f,.89f,.75f);
            Part(geometry,"LibraryLightTube",new Vector3(x,11.3f,2),new Vector3(3,.08f,.35f),Color.white);
        }
        foreach(float z in new[]{.3f,3.8f})Part(geometry,"ReadingTable",new Vector3(24.5f,8.4f,z),new Vector3(2,.8f,1.3f),wood);
        Prop(props,"LibraryIndex",new Vector3(25,9.2f,6.7f),new Vector3(.8f,.5f,.06f),CampusEscapeObject.Action.LibraryIndex,"阅读书目索引");
        Prop(props,"LibraryOldRecord",new Vector3(24.5f,8.86f,3.8f),new Vector3(.55f,.05f,.4f),CampusEscapeObject.Action.LibraryRecord,"阅读旧检修记录 · 086 号手册");
        for(int i=0;i<3;i++)
        {
            float x=38.5f+i*2;
            Prop(props,"Archive_"+i,new Vector3(x,9.05f,-2.8f),new Vector3(1.1f,2.1f,.65f),i==1?CampusEscapeObject.Action.LibraryArchive:CampusEscapeObject.Action.LibraryWrongArchive,"检索档案柜 "+new[]{"A · 024","B · 086","C · 142"}[i]).AddComponent<NavMeshObstacle>().carving=true;
            Label(geometry,new[]{"A · 024","B · 086","C · 142"}[i],new Vector3(x,10.35f,-2.35f));
        }
        Label(geometry,"三楼图书馆 · 校史与检修档案",new Vector3(33,10.8f,8.22f));
        c.eastDown=props.Find("EastDownLock");c.westDown=props.Find("WestStairLock");
        c.eastDown.GetComponent<CampusEscapeObject>().action=CampusEscapeObject.Action.LibraryStairLock;
        c.westDown.GetComponent<CampusEscapeObject>().action=CampusEscapeObject.Action.LibraryStairLock;
        c.utilityDoor=props.Find("LockedUtilityDoor");
        // Ground service perimeter: yard routes stay physically closed until emergency mode.
        Glass(geometry,"ServiceBack",new Vector3(16.2f,1.7f,-4.3f),new Vector3(56.4f,3.4f,.12f));
        Glass(geometry,"ServiceFrontLeft",new Vector3(1.6f,1.7f,12.5f),new Vector3(26.6f,3.4f,.12f));
        Glass(geometry,"ServiceFrontRight",new Vector3(33.35f,1.7f,12.5f),new Vector3(32.5f,3.4f,.12f));
        Glass(geometry,"ServiceWestOuter",new Vector3(-17.15f,1.7f,14.05f),new Vector3(.12f,3.4f,36.9f));
        Glass(geometry,"ServiceWestReturn",new Vector3(-11.7f,1.7f,22.45f),new Vector3(.12f,3.4f,19.9f));
        Glass(geometry,"ServiceWestBack",new Vector3(-14.4f,1.7f,-4.3f),new Vector3(5.6f,3.4f,.12f));
        Glass(geometry,"ServiceEastSide",new Vector3(44.4f,1.7f,2),new Vector3(.12f,3.4f,12));
        Glass(geometry,"ServiceStairBack",new Vector3(46.95f,1.7f,-2.1f),new Vector3(5.1f,3.4f,.12f));
        foreach(var pane in root.GetComponentsInChildren<Transform>())
            if(pane.name=="DownFlightGuard" && Mathf.Abs(pane.position.x-46.5f)<.01f)
            {pane.localPosition=new Vector3(46.5f,3.75f,5);pane.localScale=new Vector3(.12f,7.5f,10);}
        Glass(geometry,"GroundStairExitUpperGuard",new Vector3(46.5f,5.5f,11.2f),new Vector3(.12f,4,2.4f));
        var landing=GameObject.Find("EastStairCore").transform.Find("EastGroundLanding");LevelBuildUtil.DestroyObject(landing.gameObject);
        Part(geometry,"ServiceEastLanding",new Vector3(46.95f,-.1f,11.4f),new Vector3(5.1f,.2f,2.8f),wall);
        Part(geometry,"ServiceEastLandingRoof",new Vector3(47.95f,3.5f,11.45f),new Vector3(3.1f,.2f,2.1f),wall);
        Part(geometry,"UnderAccessFloor",new Vector3(45.4f,-.1f,3),new Vector3(2,.2f,10.2f),wall);
        c.yardDoor=Gate(props,"ServiceYardDoor",new Vector3(16,1.2f,12.5f),new Vector3(2.2f,2.4f,.18f),CampusEscapeObject.Action.ServiceExit,"庭院门：等待紧急模式");
        Part(geometry,"ServiceYardDoorHead",new Vector3(16,2.9f,12.5f),new Vector3(2.2f,1,.18f),wall);
        c.westYardDoor=Gate(props,"WestServiceExit",new Vector3(-14.4f,1.7f,32.4f),new Vector3(5.5f,3.4f,.18f),CampusEscapeObject.Action.ServiceExit,"西侧通道：等待紧急模式");
        foreach(float x in new[]{-7.5f,15f,27f,39f})Gate(props,"GroundClosed_"+x,new Vector3(x,1.2f,8),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.LibraryBarrier,"后勤非开放区域，请前往 102 配电间");
        var lights=new List<Light>();
        foreach(Vector3 at in new[]{new Vector3(3,3,10),new Vector3(28,3,10),new Vector3(-14,3,31)})
        {
            var go=new GameObject("EmergencyInteriorLight");go.transform.SetParent(geometry,false);go.transform.position=at;
            var light=go.AddComponent<Light>();light.type=LightType.Point;light.range=7;light.color=new Color(1,.3f,.09f);light.intensity=0;lights.Add(light);
        }
        c.emergencyLights=lights.ToArray();
        CampusWorldBuilder.CreateTeacher(geometry,"ServicePatrol",new Vector3(18,0,10),Quaternion.Euler(0,90,0),new[]{new Vector3(12,0,10),new Vector3(20,0,10)},1.05f,3.1f,ScrutinyAura.Role.Patrol,6,50);
        var checkpoint=new GameObject("LibraryCheckpoint").transform;checkpoint.SetParent(root,false);checkpoint.position=new Vector3(33,8.92f,10);
        var trigger=new GameObject("LibraryCheckpointTrigger");trigger.transform.SetParent(root,false);trigger.transform.position=new Vector3(33,9,10);
        var box=trigger.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(4,2,3);trigger.AddComponent<CampusCheckpoint>().checkpoint=checkpoint;
    }
    static GameObject Part(Transform p,string n,Vector3 at,Vector3 size,Color color)
    {return LevelBuildUtil.Make(PrimitiveType.Cube,p,n,at,size,Quaternion.identity,color,"Campus_Library",true);}
    static void WallSegment(Transform p,float a,float b,float z,Color c){Part(p,"LibraryWall",new Vector3((a+b)/2,9.7f,z),new Vector3(b-a,3.4f,.18f),c);}
    static GameObject Prop(Transform p,string n,Vector3 at,Vector3 size,CampusEscapeObject.Action action,string label)
    {var go=Part(p,n,at,size,new Color(.55f,.37f,.2f));var item=go.AddComponent<CampusEscapeObject>();item.action=action;item.displayName=label;return go;}
    static Transform Gate(Transform p,string n,Vector3 at,Vector3 size,CampusEscapeObject.Action action,string label)
    {var go=Prop(p,n,at,size,action,label);go.AddComponent<NavMeshObstacle>().carving=true;return go.transform;}
    static void Glass(Transform p,string n,Vector3 at,Vector3 size)
    {
        var go=Part(p,n,at,size,new Color(.56f,.72f,.76f));LevelBuildUtil.DestroyObject(go.GetComponent<PrimitiveTint>());
        var material=new Material(Shader.Find("Standard")){name="CampusArt_ChapterSecurityGlass",color=new Color(.56f,.72f,.76f,.14f)};
        material.SetFloat("_Mode",2);material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);material.SetInt("_ZWrite",0);material.EnableKeyword("_ALPHABLEND_ON");material.renderQueue=3000;
        go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    static void Label(Transform p,string text,Vector3 at)
    {var go=new GameObject("LibrarySign");go.transform.SetParent(p,false);go.transform.position=at;go.transform.rotation=Quaternion.Euler(0,180,0);var mesh=go.AddComponent<TextMesh>();mesh.font=Resources.Load<Font>("UIFont");mesh.text=text;mesh.characterSize=.06f;mesh.fontSize=40;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=Color.white;go.AddComponent<CampusWorldSign>();}
    void Start()
    {
        subtitles=GetComponent<CampusExamChapter>();GameProgress.Ensure().SaveApplied+=Restore;
        foreach(var light in UnityCompat.FindAll<Light>())if(light.name=="InteriorLight" && light.transform.position.y<4 && light.transform.position.x> -20 && light.transform.position.x<55 && light.transform.position.z<53)originalLighting[light]=light.intensity;
        Restore();
    }
    void OnDestroy(){if(GameProgress.Instance!=null)GameProgress.Instance.SaveApplied-=Restore;}
    public void Restore()
    {
        Open(eastDown,State.downOpen);Open(westDown,State.downOpen);Open(libraryShortcut,State.manual);Open(returnDoor,State.manual);
        Open(utilityDoor,State.utilityOpen);Open(yardDoor,State.emergency);Open(westYardDoor,State.emergency);
        foreach(var light in emergencyLights)if(light!=null)light.intensity=State.emergency?1.4f:0;
        foreach(var pair in originalLighting)if(pair.Key!=null)pair.Key.intensity=State.emergency?pair.Value*.12f:pair.Value;
        GetComponent<CampusEscapeDirector>().RestoreLibraryProgress(State.manual,State.key,State.utilityOpen,State.emergency);
    }
    static void Open(Transform door,bool open){if(door==null)return;door.GetComponent<Collider>().enabled=!open;door.GetComponent<NavMeshObstacle>().enabled=!open;door.GetComponent<Renderer>().enabled=!open;}
    public bool Use(CampusEscapeObject item)
    {
        var action=item.action;
        if(action==CampusEscapeObject.Action.GateRelease){Say("庭院已开放。校门控制流程将在下一阶段接通。");return true;}
        if(action!=CampusEscapeObject.Action.UtilityDoor && action!=CampusEscapeObject.Action.PowerSwitch && (int)action<(int)CampusEscapeObject.Action.LibraryIndex)return false;
        if(!GetComponent<CampusCorridorChapter>().State.circuit){Say("先完成 203 设备室回路，才能调查三楼与后勤。");return true;}
        switch(action)
        {
            case CampusEscapeObject.Action.LibraryIndex:State.indexRead=true;Say("书目索引：紧急门禁档案按现行检修手册的书号归档。请先查阅阅览桌上的旧检修记录，再选择对应档案柜。");break;
            case CampusEscapeObject.Action.LibraryRecord:State.recordRead=true;Say("旧检修记录：024 号手册已经废止，142 号用于食堂设备。现行教学楼门禁采用 086 号手册。");break;
            case CampusEscapeObject.Action.LibraryWrongArchive:Say("这个柜子不是现行教学楼门禁档案。请核对索引和旧检修记录。");break;
            case CampusEscapeObject.Action.LibraryArchive:
                if(State.manual){Say("紧急说明和维修钥匙已经收好，可以随时回看索引。");break;}
                if(!State.indexRead || !State.recordRead){Say("先对照书目索引和阅览桌的旧检修记录，确认档案归属。");break;}
                State.manual=State.key=State.downOpen=true;Restore();Say("找到紧急说明与维修钥匙。东西下楼通道和图书馆回访捷径已开放；一楼 102 配电间需要同时出示说明和钥匙。");break;
            case CampusEscapeObject.Action.LibraryStairLock:Say(State.downOpen?"下楼通道已开放。":"下楼门禁需要图书馆的紧急说明。");break;
            case CampusEscapeObject.Action.LibraryShortcut:Say(State.manual?"后廊与 303 回访通道已开放，可绕回前廊。":"先找到现行门禁档案。");break;
            case CampusEscapeObject.Action.UtilityDoor:
                if(!State.manual || !State.key){Say("配电间需要维修钥匙和紧急门禁说明，缺一不可。");break;}
                State.utilityOpen=true;Restore();Say("配电间已解锁。内部开关可启动紧急疏散模式。");break;
            case CampusEscapeObject.Action.PowerSwitch:
                if(!State.utilityOpen || !State.manual || !State.key){Say("先完成图书馆档案调查并打开配电间。");break;}
                if(State.emergency){Say("紧急模式已经启用，不会再次关闭已开放的通路。");break;}
                State.emergency=true;Restore();foreach(var teacher in UnityCompat.FindAll<TeacherAgent>())teacher.InvestigateNoise(item.transform.position,22);
                Say("广播：紧急疏散模式已启动。应急照明开启，庭院前门与西侧通道解除封锁。附近老师正在调查。");break;
            case CampusEscapeObject.Action.ServiceExit:Say(State.emergency?"庭院通道已开放。":"庭院门尚未解锁，请在配电间启动紧急模式。");break;
            case CampusEscapeObject.Action.LibraryBarrier:Say(item.displayName);break;
            default:return false;
        }
        return true;
    }
    void Say(string text){if(subtitles!=null)subtitles.ShowSubtitle(text,10);}
}

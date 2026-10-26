using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CampusCorridorState
{
    public bool notice, conversation, card, roomOpen, record, lighting, alarm=true, doorPower, circuit;
}

/// <summary>Stages B/C. All input arrives through the existing director.</summary>
public class CampusCorridorChapter : MonoBehaviour
{
    public Transform equipmentDoor, eastGate;
    public Vector3 eastClosedPosition;
    float animationUntil;
    CampusFlowController flow;
    CampusExamChapter exam;
    AudioClip mechanismSound;
    public CampusCorridorState State { get { return GameProgress.Ensure().corridorChapter; } }
    public string Objective { get { return !State.card?"调查二楼走廊，寻找 203 通行卡":!State.roomOpen?"用通行卡打开 203 设备室":!State.circuit?"阅读维修记录，恢复东楼梯门禁供电":"东楼梯已开启，前往三楼图书馆"; } }
    public string Detail { get { return !State.card?"询问走廊同学或查看公告栏；203 附近有储物柜":!State.roomOpen?"203 前门使用通行卡；其他楼层暂未开放":!State.circuit?"设备室内三路开关可按 E 切换，按维修记录设置后在控制台确认":"向走廊东端前进，走右侧上行楼梯；一楼仍未解锁"; } }
    public static void Build(Transform root)
    {
        var c=root.gameObject.AddComponent<CampusCorridorChapter>();
        var props=root.Find("EscapeObjectives");
        c.equipmentDoor=Gate(props,"Equipment203Door",new Vector3(15,5.2f,8),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.EquipmentDoor,"刷卡打开 203");
        c.eastGate=Gate(props,"EastAccessGate",new Vector3(44.4f,5.7f,10.2f),new Vector3(.18f,3.4f,4.4f),CampusEscapeObject.Action.EastAccessGate,"检查东楼梯门禁");
        c.eastClosedPosition=c.eastGate.position;
        Gate(props,"WestStairLock",new Vector3(-14.4f,5.7f,20.5f),new Vector3(5.2f,3.4f,.18f),CampusEscapeObject.Action.CorridorBarrier,"西楼梯：应急封锁中");
        Gate(props,"EastDownLock",new Vector3(48,5.4f,0),new Vector3(3,2.8f,.18f),CampusEscapeObject.Action.CorridorBarrier,"下楼通道：需三楼紧急说明");
        var shell=new GameObject("ChapterCorridorEnclosure").transform;shell.SetParent(root,false);
        // Full-height glazing prevents jumping from a rail or escaping between classrooms.
        Glass(shell,"FrontGalleryGlazing",new Vector3(17.3f,6.25f,12.5f),new Vector3(58.4f,2.5f,.12f));
        Glass(shell,"RearAccessGlazing",new Vector3(16.2f,5.7f,-4.3f),new Vector3(56.4f,3.4f,.12f));
        Glass(shell,"WestOuterGlazing",new Vector3(-17.15f,5.7f,8.1f),new Vector3(.12f,3.4f,25));
        Glass(shell,"WestReturnGlazing",new Vector3(-11.7f,5.7f,16.5f),new Vector3(.12f,3.4f,8.2f));
        Glass(shell,"WestEndGlazing",new Vector3(-14.4f,5.7f,-4.3f),new Vector3(5.6f,3.4f,.12f));
        Glass(shell,"EastRoomEdgeGlazing",new Vector3(44.4f,5.7f,2),new Vector3(.12f,3.4f,12));
        var landing=Part(shell,"EastGalleryLanding",new Vector3(45.4f,3.9f,11.2f),new Vector3(2,.2f,2.4f));
        Part(shell,"EastGalleryLandingRoof",new Vector3(45.4f,7.5f,11.2f),new Vector3(2,.2f,2.4f));
        // Enclose the descending flight, so its middle landing cannot be bypassed by jumping.
        foreach(float x in new[]{46.5f,49.5f})Glass(shell,"DownFlightGuard",new Vector3(x,3.75f,6.2f),new Vector3(.12f,7.5f,12.4f));
        Glass(shell,"EastLandingEnd",new Vector3(53.7f,5.7f,-1),new Vector3(.12f,3.4f,2.2f));
        Glass(shell,"ThirdLandingRear",new Vector3(47.45f,9.7f,9.9f),new Vector3(6.1f,3.4f,.12f));
        Glass(shell,"ThirdTopReturn",new Vector3(44.5f,9.7f,9),new Vector3(.12f,3.4f,2));
        var oldRoof=GameObject.Find("EastStairCore").transform.Find("EastMiddleRoof");if(oldRoof!=null)LevelBuildUtil.DestroyObject(oldRoof.gameObject);
        Part(shell,"MiddleLandingCanopy",new Vector3(50,7.5f,-1),new Vector3(7.2f,.2f,2));
        Part(shell,"AccessEndCanopy",new Vector3(45.4f,7.5f,-1.6f),new Vector3(2,.2f,.8f));
        Glass(shell,"EastLandingBack",new Vector3(49,5.7f,-2.1f),new Vector3(9.6f,3.4f,.12f));
        foreach(float x in new[]{50.5f,53.6f})Glass(shell,"UpFlightGuard",new Vector3(x,7.75f,4.95f),new Vector3(.12f,7.5f,9.9f));
        Glass(shell,"ThirdFrontGlazing",new Vector3(21.3f,10.25f,12.5f),new Vector3(66.4f,2.5f,.12f));
        Glass(shell,"ThirdBackGlazing",new Vector3(16,9.7f,-32.2f),new Vector3(57,3.4f,.12f));
        foreach(float x in new[]{-12.5f,44.5f})Glass(shell,"ThirdSideGlazing",new Vector3(x,9.7f,-12),new Vector3(.12f,3.4f,40));
        Glass(shell,"ThirdWestEnd",new Vector3(-11.9f,9.7f,10.2f),new Vector3(.12f,3.4f,4.4f));
        Glass(shell,"ThirdEastEnd",new Vector3(54.5f,9.7f,11.2f),new Vector3(.12f,3.4f,2.6f));
        foreach(float z in new[]{-8.4f,-16f})Glass(shell,"ThirdCourtyardEdge",new Vector3(16,9.7f,z),new Vector3(48.8f,3.4f,.12f));
        foreach(float x in new[]{-11.6f,-8.4f,40.4f,43.6f})Glass(shell,"ThirdBridgeGuard",new Vector3(x,9.7f,-12.2f),new Vector3(.12f,3.4f,7.6f));
        foreach(float x in new[]{3f,27f,39f})Gate(props,"ClosedClassroom_"+x,new Vector3(x,5.2f,8),new Vector3(1.8f,2.4f,.18f),CampusEscapeObject.Action.CorridorBarrier,"教室关闭，寻找 203");
        Prop(props,"CorridorNotice",new Vector3(-3,5.2f,9.1f),new Vector3(1.2f,.7f,.08f),CampusEscapeObject.Action.CorridorNotice,"查看公告栏");
        Prop(props,"AccessCardLocker",new Vector3(12,5,9.1f),new Vector3(.6f,1.8f,.5f),CampusEscapeObject.Action.AccessCard,"调查 203 旁储物柜");
        Prop(props,"EquipmentRecord",new Vector3(12,5.03f,3.4f),new Vector3(.5f,.04f,.35f),CampusEscapeObject.Action.EquipmentRecord,"阅读维修记录");
        Prop(props,"LightingCircuit",new Vector3(13,5,-3.6f),new Vector3(.35f,.55f,.2f),CampusEscapeObject.Action.LightingCircuit,"照明回路");
        Prop(props,"AlarmCircuit",new Vector3(15,5,-3.6f),new Vector3(.35f,.55f,.2f),CampusEscapeObject.Action.AlarmCircuit,"警报回路");
        Prop(props,"DoorPowerCircuit",new Vector3(17,5,-3.6f),new Vector3(.35f,.55f,.2f),CampusEscapeObject.Action.DoorPowerCircuit,"门禁供电回路");
        Prop(props,"CircuitConfirm",new Vector3(15,5,-1.6f),new Vector3(.6f,.6f,.2f),CampusEscapeObject.Action.CircuitConfirm,"确认回路设置");
        var room=GameObject.Find("Classroom_203").transform;
        foreach(var child in room.GetComponentsInChildren<Transform>())
            if(child!=room && (child.name=="StudyDesk" || child.name=="DeskSupport" || child.name=="RoomCupboard"))LevelBuildUtil.DestroyObject(child.gameObject);
        foreach(float x in new[]{-3f,3f})
        {
            Part(room,"RepairBench",new Vector3(x,.45f,0),new Vector3(1.1f,.9f,3.4f));
            Part(room,"ToolRack",new Vector3(x,2,-2),new Vector3(1.1f,.9f,.2f));
        }
        Label(props,"照明",new Vector3(13,5.65f,-3.45f));
        Label(props,"警报",new Vector3(15,5.65f,-3.45f));
        Label(props,"门禁供电",new Vector3(17,5.65f,-3.45f));
        Label(props,"203 · 维修设备室",new Vector3(15,6.8f,8.25f));
        Label(props,"203 备用通行卡",new Vector3(12,6.2f,9.15f));
        // Old v11 reward is deferred until the library stage; no early utility key.
        foreach(string n in new[]{"MaintenanceKey","MaintenanceKeyStand","EvacuationPlan"}){var old=GameObject.Find(n);if(old!=null)old.SetActive(false);}
        CampusCorridorNPC.Build(props,"Student_Lin",new Vector3(-7,4,9),"林同学",true);
        CampusCorridorNPC.Build(props,"Student_Zhou",new Vector3(25,4,9),"周同学",false);
        CampusCorridorNPC.Build(props,"Student_Wu",new Vector3(36,4,9),"吴同学",false);
    }
    static GameObject Part(Transform p,string n,Vector3 at,Vector3 size)
    {return LevelBuildUtil.Make(PrimitiveType.Cube,p,n,at,size,Quaternion.identity,new Color(.26f,.39f,.42f),"Campus_Equipment",true);}
    static void Glass(Transform p,string n,Vector3 at,Vector3 size)
    {
        var go=LevelBuildUtil.Make(PrimitiveType.Cube,p,n,at,size,Quaternion.identity,new Color(.56f,.72f,.76f),"Campus_SecurityGlass",true);
        LevelBuildUtil.DestroyObject(go.GetComponent<PrimitiveTint>());
        var material=new Material(Shader.Find("Standard")){name="CampusArt_ChapterSecurityGlass",color=new Color(.56f,.72f,.76f,.14f)};
        material.SetFloat("_Mode",2);material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);material.SetInt("_ZWrite",0);material.EnableKeyword("_ALPHABLEND_ON");material.renderQueue=3000;
        go.GetComponent<Renderer>().sharedMaterial=material;
        go.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    static GameObject Prop(Transform p,string n,Vector3 at,Vector3 size,CampusEscapeObject.Action action,string label)
    {
        var go=Part(p,n,at,size);var item=go.AddComponent<CampusEscapeObject>();item.action=action;item.displayName=label;return go;
    }
    static Transform Gate(Transform p,string n,Vector3 at,Vector3 size,CampusEscapeObject.Action action,string label)
    {var go=Prop(p,n,at,size,action,label);var obstacle=go.AddComponent<NavMeshObstacle>();obstacle.carving=true;return go.transform;}
    static void Label(Transform p,string text,Vector3 at)
    {
        var go=new GameObject("ChapterSign");go.transform.SetParent(p,false);go.transform.position=at;go.transform.rotation=Quaternion.Euler(0,180,0);
        var mesh=go.AddComponent<TextMesh>();mesh.text=text;mesh.font=Resources.Load<Font>("UIFont");mesh.characterSize=.055f;mesh.fontSize=40;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=Color.white;go.AddComponent<CampusWorldSign>();
    }
    void Start()
    {
        flow=UnityCompat.FindOne<CampusFlowController>();exam=GetComponent<CampusExamChapter>();
        GameProgress.Ensure().SaveApplied+=Restore;
        mechanismSound=AudioClip.Create("LocalMechanismFeedback",4410,1,22050,false);var samples=new float[4410];
        for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(i*2*Mathf.PI*660/22050)*.15f*(1-i/(float)samples.Length);
        mechanismSound.SetData(samples,0);Restore();
    }
    void OnDestroy(){if(GameProgress.Instance!=null)GameProgress.Instance.SaveApplied-=Restore;if(mechanismSound!=null)Destroy(mechanismSound);}
    public void Restore()
    {
        SetDoor(equipmentDoor,State.roomOpen);SetDoor(eastGate,State.circuit);
        eastGate.position=eastClosedPosition;animationUntil=0;
        RefreshSwitch("LightingCircuit",State.lighting);RefreshSwitch("AlarmCircuit",State.alarm);RefreshSwitch("DoorPowerCircuit",State.doorPower);
    }
    static void SetDoor(Transform door,bool open)
    {door.GetComponent<Collider>().enabled=!open;door.GetComponent<NavMeshObstacle>().enabled=!open;door.GetComponent<Renderer>().enabled=!open;}
    void RefreshSwitch(string name,bool on)
    {
        var item=transform.Find("EscapeObjectives/"+name).GetComponent<CampusEscapeObject>();
        string label=name=="LightingCircuit"?"照明":name=="AlarmCircuit"?"警报":"门禁供电";
        item.displayName=label+" · "+(on?"开":"关")+(State.circuit?"（已锁定）":" · E 切换");
        Color color=on?new Color(.15f,.7f,.35f):new Color(.55f,.18f,.12f);
        // Bootstrap can apply tints after this Start; keep both sources consistent.
        var tint=item.GetComponent<PrimitiveTint>();if(tint!=null)tint.color=color;
        item.GetComponent<Renderer>().material.color=color;
    }
    public bool Use(CampusEscapeObject item)
    {
        if((int)item.action<(int)CampusEscapeObject.Action.CorridorNotice)return false;
        if(exam==null || !exam.SideOpen){Say("请先用钥匙打开 201 侧门。");return true;}
        switch(item.action)
        {
            case CampusEscapeObject.Action.CorridorNotice:State.notice=true;Say("公告：203 改作维修设备室。备用通行卡留在 203 门旁储物柜；东楼梯等待回路检修。");break;
            case CampusEscapeObject.Action.CorridorNPC:
                State.conversation=true;var npc=item.GetComponent<CampusCorridorNPC>();
                Say(npc.characterName+"："+(npc.mainClue?"203 门旁的储物柜里有备用通行卡。里面的维修记录能告诉你怎么恢复东楼梯。":"巡逻老师还在前廊。广播按钮可以让附近的老师去调查，别在唯一出口停留。"));break;
            case CampusEscapeObject.Action.AccessCard:
                State.card=true;item.displayName="储物柜：通行卡已取走";Say("获得 203 通行卡。对准 203 门锁按 E 刷卡。");break;
            case CampusEscapeObject.Action.EquipmentDoor:
                if(!State.card){Say("203 门锁需要通行卡。查看公告栏或问问同学。");break;}
                State.roomOpen=true;Restore();exam.ShowEvent("room203",CampusVoice.Feedback,"通行卡验证通过。203 设备室已开放。");break;
            case CampusEscapeObject.Action.EquipmentRecord:
                if(!State.roomOpen){Say("先解锁 203。");break;}State.record=true;Say("维修记录：照明开启、警报关闭、门禁供电开启；完成后到后方控制台确认。下楼通道另需三楼紧急说明。");break;
            case CampusEscapeObject.Action.LightingCircuit:if(CanSwitch())State.lighting=!State.lighting;Restore();break;
            case CampusEscapeObject.Action.AlarmCircuit:if(CanSwitch())State.alarm=!State.alarm;Restore();break;
            case CampusEscapeObject.Action.DoorPowerCircuit:if(CanSwitch())State.doorPower=!State.doorPower;Restore();break;
            case CampusEscapeObject.Action.CircuitConfirm:
                if(State.circuit){Say("回路稳定，东楼梯已开放。");break;}
                if(!State.roomOpen || !State.record){Say("先阅读设备室的维修记录。");break;}
                if(!State.lighting || State.alarm || !State.doorPower){Say("回路校验失败：请核对照明、警报与门禁供电三个开关。");break;}
                State.circuit=true;Restore();eastGate.GetComponent<Renderer>().enabled=true;animationUntil=Time.time+.8f;AudioSource.PlayClipAtPoint(mechanismSound,item.transform.position,.5f);exam.ShowEvent("circuit",CampusVoice.Feedback,"门禁供电恢复。东楼梯锁舌已释放，可以前往三楼。");break;
            case CampusEscapeObject.Action.EastAccessGate:Say(State.circuit?"东楼梯已开放，沿右侧上楼。":"东楼梯未供电，请在 203 完成检修。");break;
            case CampusEscapeObject.Action.CorridorBarrier:Say(item.displayName);break;
            default:return false;
        }
        return true;
    }
    void Update()
    {
        if(animationUntil<=0)return;
        eastGate.position=Vector3.Lerp(eastClosedPosition,eastClosedPosition+Vector3.up*3.5f,1-(animationUntil-Time.time)/.8f);
        if(Time.time>=animationUntil){eastGate.GetComponent<Renderer>().enabled=false;animationUntil=0;}
    }
    bool CanSwitch(){if(!State.roomOpen || State.circuit){Say("当前回路已锁定，不能切换。");return false;}Say("开关已切换。按维修记录设置三路，再到控制台确认。");return true;}
    void Say(string text){var narration=GetComponent<CampusNarrative>();if(narration!=null)narration.Emit(null,text.Contains("同学：")?CampusVoice.NPC:CampusVoice.Feedback,text,8,false);else if(exam!=null)exam.ShowSubtitle(text,8);}
}

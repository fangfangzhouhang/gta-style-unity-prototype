using UnityEngine;

/// <summary>Observe -> key -> utility room -> power -> gate. Progress survives capture within a run.</summary>
public class CampusEscapeDirector : MonoBehaviour
{
    public bool PlanRead { get; private set; }
    public bool HasKey { get; private set; }
    public bool UtilityOpen { get; private set; }
    public bool PowerOff { get; private set; }
    public bool GateOpen { get; private set; }
    public Transform gateBarrier;
    CampusFlowController flow;
    CampusEscapeObject[] objects;
    float nextBell;
    public string Objective
    {
        get {
            var chapter=GetComponent<CampusExamChapter>();
            if(chapter!=null && !chapter.SideOpen)return chapter.Objective;
            if(!PlanRead)return "调查二楼考场的疏散示意图";
            if(!HasKey)return "去二楼 203 教室找维修钥匙";
            if(!PowerOff)return "前往一楼配电间，关闭门禁警报";
            if(!GateOpen)return "前往校门控制箱，打开出口";
            return "校门已开启，离开校园";
        }
    }
    public string Detail { get { return GetComponent<CampusExamChapter>()!=null && !GetComponent<CampusExamChapter>().SideOpen?"后排试卷、座位表、密码柜均可按 E 调查；退格键跳过字幕":!PlanRead?"201 后排墙边按 E 查看；教室、外廊、楼梯均可探索":!HasKey?"从外廊进入 203；广播按钮能把附近老师引开":!PowerOff?"钥匙可开一楼配电间；东楼梯与西楼梯都能下楼":"已找到的线索会保留；被抓后可从检查点继续"; } }
    void Start()
    {
        flow=UnityCompat.FindOne<CampusFlowController>();
        var existing=transform.Find("EscapeObjectives");
        if(existing!=null){objects=existing.GetComponentsInChildren<CampusEscapeObject>(true);gateBarrier=existing.Find("LockedCampusGate");return;}
        var root=new GameObject("EscapeObjectives").transform;root.SetParent(transform,false);
        Prop(root,"EvacuationPlan",CampusEscapeObject.Action.EvacuationPlan,new Vector3(-9.6f,5.15f,-2.9f),new Color(.9f,.85f,.57f),"疏散图");
        Prop(root,"MaintenanceKey",CampusEscapeObject.Action.MaintenanceKey,new Vector3(15,4.95f,3.9f),new Color(.95f,.75f,.12f),"维修钥匙");
        Prop(root,"PowerSwitch",CampusEscapeObject.Action.PowerSwitch,new Vector3(3,1.15f,3.3f),new Color(.28f,.55f,.64f),"配电开关");
        Prop(root,"GateRelease",CampusEscapeObject.Action.GateRelease,new Vector3(-10.6f,1.1f,50),new Color(.25f,.68f,.38f),"校门控制箱");
        Prop(root,"DiversionBell",CampusEscapeObject.Action.DiversionBell,new Vector3(23,5.05f,10.1f),new Color(.8f,.35f,.15f),"广播按钮");
        var door=LevelBuildUtil.Make(PrimitiveType.Cube,root,"LockedUtilityDoor",new Vector3(3,1.1f,8),new Vector3(1.65f,2.2f,.14f),Quaternion.identity,new Color(.25f,.38f,.43f),"Campus_UtilityDoor",true);
        var interaction=door.AddComponent<CampusEscapeObject>();interaction.action=CampusEscapeObject.Action.UtilityDoor;interaction.displayName="配电间门锁";
        door.AddComponent<UnityEngine.AI.NavMeshObstacle>().carving=true;
        var barrier=LevelBuildUtil.Make(PrimitiveType.Cube,root,"LockedCampusGate",new Vector3(-14,1.5f,52.5f),new Vector3(4.8f,3,.18f),Quaternion.identity,new Color(.24f,.27f,.25f),"Campus_GateBars",true);
        gateBarrier=barrier.transform;
        barrier.AddComponent<UnityEngine.AI.NavMeshObstacle>().carving=true;
        objects=GetComponentsInChildren<CampusEscapeObject>();
    }
    // Author the same objects in edit mode so scene dependencies and navigation are complete.
    public void BuildObjects(){Start();}
    void Awake(){objects=GetComponentsInChildren<CampusEscapeObject>();}
    void OnEnable(){if(Application.isPlaying)flow=UnityCompat.FindOne<CampusFlowController>();}
    void Update()
    {
        if(flow==null)flow=UnityCompat.FindOne<CampusFlowController>();
        if(flow==null || flow.IsBusy || Time.timeScale==0 || (PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen) || (flow.mapUi!=null && flow.mapUi.IsOpen))return;
        if(GetComponent<CampusExamChapter>()!=null && GetComponent<CampusExamChapter>().KeypadOpen)return;
        if(Input.GetKeyDown(KeyCode.E)){var item=Nearby();if(item!=null)TryUse(item);}
    }
    public CampusEscapeObject Nearby()
    {
        if(flow==null || flow.player==null || !flow.player.GetComponent<ThirdPersonController>().controlEnabled)return null;
        CampusEscapeObject best=null;float nearest=2.2f;
        if(objects==null || objects.Length==0)objects=GetComponentsInChildren<CampusEscapeObject>();
        foreach(var item in objects)
        {
            if(item==null || !item.gameObject.activeInHierarchy)continue;
            var chapter=GetComponent<CampusExamChapter>();
            if(chapter!=null && ((item.action==CampusEscapeObject.Action.ExamPaper && chapter.State.paper) || (item.action==CampusEscapeObject.Action.ExamChart && chapter.State.chart) || (item.action==CampusEscapeObject.Action.ExamCabinet && chapter.State.key) || (item.action==CampusEscapeObject.Action.ExamSideDoor && chapter.SideOpen)))continue;
            if((item.action==CampusEscapeObject.Action.EvacuationPlan && PlanRead) || (item.action==CampusEscapeObject.Action.MaintenanceKey && HasKey) || (item.action==CampusEscapeObject.Action.UtilityDoor && UtilityOpen) || (item.action==CampusEscapeObject.Action.PowerSwitch && PowerOff) || (item.action==CampusEscapeObject.Action.GateRelease && GateOpen))continue;
            float distance=Vector3.Distance(flow.player.position,item.transform.position);
            if(distance>=nearest)continue;
            if(Physics.Linecast(flow.player.position,item.transform.position,out RaycastHit hit,~0,QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(item.transform))continue;
            best=item;nearest=distance;
        }
        return best;
    }
    public string NearbyPrompt(){var item=Nearby();return item==null?"":"E · "+item.displayName;}
    public bool TryUse(CampusEscapeObject item)
    {
        if(item==null || item!=Nearby())return false;
        var chapter=GetComponent<CampusExamChapter>();
        if(chapter!=null && chapter.Use(item))return true;
        bool done=false;
        switch(item.action)
        {
            case CampusEscapeObject.Action.EvacuationPlan: PlanRead=true;done=true;break;
            case CampusEscapeObject.Action.MaintenanceKey:
                if(PlanRead){HasKey=true;item.gameObject.SetActive(false);done=true;}break;
            case CampusEscapeObject.Action.UtilityDoor:
                if(HasKey){UtilityOpen=true;item.GetComponent<Collider>().enabled=false;item.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled=false;item.transform.rotation=Quaternion.Euler(0,90,0);done=true;}break;
            case CampusEscapeObject.Action.PowerSwitch: if(HasKey && UtilityOpen){PowerOff=true;done=true;}break;
            case CampusEscapeObject.Action.GateRelease:
                if(PowerOff){GateOpen=true;if(gateBarrier!=null)gateBarrier.gameObject.SetActive(false);done=true;}break;
            case CampusEscapeObject.Action.DiversionBell:
                if(Time.time>=nextBell){nextBell=Time.time+8;foreach(var teacher in UnityCompat.FindAll<TeacherAgent>())teacher.InvestigateNoise(item.transform.position,22);done=true;}break;
        }
        if(flow!=null)flow.ShowTip(done?"线索已更新":"暂时无法使用",Objective);
        return done;
    }
    static void Prop(Transform root,string name,CampusEscapeObject.Action action,Vector3 at,Color color,string label)
    {
        var go=LevelBuildUtil.Make(PrimitiveType.Cube,root,name,at,action==CampusEscapeObject.Action.EvacuationPlan?new Vector3(.55f,.4f,.025f):new Vector3(.28f,.26f,.16f),Quaternion.identity,color,"Campus_Clue",false);
        var item=go.AddComponent<CampusEscapeObject>();item.action=action;item.displayName=label;
        var pedestal=LevelBuildUtil.Make(PrimitiveType.Cube,root,name+"Stand",at-new Vector3(0,.55f,0),new Vector3(.4f,.7f,.4f),Quaternion.identity,new Color(.38f,.41f,.38f),"Campus_ClueStand",true);
        if(action==CampusEscapeObject.Action.EvacuationPlan)LevelBuildUtil.DestroyObject(pedestal);
    }
}

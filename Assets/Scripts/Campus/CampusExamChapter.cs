using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CampusExamState
{
    public bool paper, chart, key, sideOpen, introduction, window, corridorIntroduction;
}

/// <summary>Stage A only. Input is dispatched by CampusEscapeDirector.</summary>
public class CampusExamChapter : MonoBehaviour
{
    public Transform sideDoor;
    CampusFlowController flow;
    CampusEscapeObject cabinet;
    public bool KeypadOpen { get; private set; }
    public CampusExamState State { get { return GameProgress.Ensure().examChapter; } }
    string digits="", feedback="", subtitle="";
    float subtitleUntil;
    public AudioSource voice;
    public AudioClip introductionAudio;
    public bool SideOpen { get { return State.sideOpen; } }
    public string Objective { get { return !State.key?"调查试卷和座位表，打开密码柜":!SideOpen?"用钥匙打开 201 西侧门":"离开 201，调查二楼走廊"; } }
    public static void Build(Transform root)
    {
        var chapter=root.gameObject.AddComponent<CampusExamChapter>();
        var parent=root.Find("EscapeObjectives");
        chapter.sideDoor=Door(parent,"Chapter201SideDoor",new Vector3(-12,5.1f,-1.2f),new Vector3(.18f,2.2f,2.2f),CampusEscapeObject.Action.ExamSideDoor);
        Door(parent,"Chapter201FrontDoor",new Vector3(-7.5f,5.2f,8),new Vector3(2.4f,2.4f,.18f),CampusEscapeObject.Action.ExamFrontDoor);
        Prop(parent,"ChapterPaper",new Vector3(-8.6f,4.85f,-2.8f),"调查桌面试卷",CampusEscapeObject.Action.ExamPaper,new Vector3(.5f,.03f,.35f));
        Prop(parent,"ChapterSeatChart",new Vector3(-6.6f,5.35f,-3.7f),"调查座位表",CampusEscapeObject.Action.ExamChart,new Vector3(.55f,.6f,.04f));
        Prop(parent,"ChapterCabinet",new Vector3(-10.8f,5,-2.7f),"调查密码柜",CampusEscapeObject.Action.ExamCabinet,new Vector3(.7f,1.8f,.5f));
        Prop(parent,"ChapterWindow",new Vector3(-3.2f,5.2f,2),"查看锁死的窗户",CampusEscapeObject.Action.ExamWindow,new Vector3(.02f,.1f,.1f));
        var clock=LevelBuildUtil.Make(PrimitiveType.Cylinder,root,"StoppedExamClock",new Vector3(-7.5f,6.9f,-3.8f),new Vector3(.5f,.025f,.5f),Quaternion.Euler(90,0,0),Color.white,"Campus_Clock",false);
        LevelBuildUtil.Make(PrimitiveType.Cube,root,"StoppedClockHand",new Vector3(-7.5f,6.98f,-3.74f),new Vector3(.02f,.17f,.02f),Quaternion.identity,Color.black,"Campus_ClockHand",false);
        LevelBuildUtil.Make(PrimitiveType.Cube,root,"ClueDesk",new Vector3(-8.6f,4.4f,-2.8f),new Vector3(.8f,.8f,.5f),Quaternion.identity,new Color(.4f,.3f,.2f),"Campus_ClueDesk",true);
        // Preserve the old shortcut component for future gated reuse, but it cannot escape 201.
        var vent=GameObject.Find("VentEntry");if(vent!=null)vent.GetComponent<ZoneTeleporter>().enabled=false;
    }
    static Transform Door(Transform p,string name,Vector3 at,Vector3 size,CampusEscapeObject.Action action)
    {
        var go=Prop(p,name,at,"检查 201 门锁",action,size);
        go.AddComponent<NavMeshObstacle>().carving=true;return go.transform;
    }
    static GameObject Prop(Transform p,string name,Vector3 at,string label,CampusEscapeObject.Action action,Vector3 size)
    {
        var go=LevelBuildUtil.Make(PrimitiveType.Cube,p,name,at,size,Quaternion.identity,new Color(.55f,.43f,.27f),"Campus_Chapter201",true);
        var item=go.AddComponent<CampusEscapeObject>();item.action=action;item.displayName=label;return go;
    }
    void Start()
    {
        flow=UnityCompat.FindOne<CampusFlowController>();
        GameProgress.Ensure().SaveApplied+=Restore;
        cabinet=GameObject.Find("ChapterCabinet").GetComponent<CampusEscapeObject>();
        Restore();
        if(!State.introduction){State.introduction=true;Say("主角：怎么还没有下课？这个钟，好像一直没动过。\n广播：考试尚未结束，请留在座位附近。",10);if(voice!=null && introductionAudio!=null)voice.PlayOneShot(introductionAudio);}
    }
    void OnDestroy(){if(GameProgress.Instance!=null)GameProgress.Instance.SaveApplied-=Restore;}
    public void Restore()
    {
        if(sideDoor==null)return;
        sideDoor.GetComponent<Collider>().enabled=!SideOpen;
        sideDoor.GetComponent<NavMeshObstacle>().enabled=!SideOpen;
        sideDoor.localRotation=Quaternion.Euler(0,SideOpen?90:0,0);
        sideDoor.GetComponent<Renderer>().enabled=!SideOpen;
    }
    public bool Use(CampusEscapeObject item)
    {
        switch(item.action)
        {
            case CampusEscapeObject.Action.ExamPaper:State.paper=true;Say("桌面试卷：柜锁检修提示——按第 2、1、3 排的顺序，读取座位表上的座位数。",8);break;
            case CampusEscapeObject.Action.ExamChart:State.chart=true;Say("座位表：第 1 排 1 席；第 2 排 4 席；第 3 排 6 席。\n这是旧考场的排座记录，不是当前课桌总数。",8);break;
            case CampusEscapeObject.Action.ExamCabinet:
                if(State.key){Say("柜子已经空了，侧门钥匙在口袋里。",4);break;}
                KeypadOpen=true;digits="";feedback="输入三位密码";SetControl(false);break;
            case CampusEscapeObject.Action.ExamSideDoor:
                if(!State.key){Say("侧门锁着。钥匙可能在后排密码柜里。",4);break;}
                State.sideOpen=true;Restore();Say("终于有办法了。但门外似乎还有人。",5);break;
            case CampusEscapeObject.Action.ExamFrontDoor:Say("前门已从外侧锁闭，请寻找教室侧门钥匙。",4);break;
            case CampusEscapeObject.Action.ExamWindow:State.window=true;Say("明明能看到外面，却没有一扇窗能打开。",5);break;
            default:return false;
        }
        return true;
    }
    public bool SubmitCode(string code)
    {
        if(!KeypadOpen || flow==null || flow.IsBusy || cabinet==null || Vector3.Distance(flow.player.position,cabinet.transform.position)>2.2f)return false;
        if(code!="416"){feedback="密码不对。再核对排号与座位数。";return false;}
        State.key=true;CloseKeypad();Say("柜门打开了。获得：201 侧门钥匙。",5);return true;
    }
    void SetControl(bool enabled)
    {
        if(flow==null)return;
        flow.player.GetComponent<ThirdPersonController>().controlEnabled=enabled;
        Cursor.lockState=enabled?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!enabled;
    }
    public void CloseKeypad(){KeypadOpen=false;if(flow!=null && !flow.IsBusy)SetControl(true);}
    void Update()
    {
        if(KeypadOpen && (flow==null || flow.IsBusy || Vector3.Distance(flow.player.position,cabinet.transform.position)>2.2f))CloseKeypad();
        if(Time.timeScale==0 || (PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen) || (flow!=null && flow.mapUi!=null && flow.mapUi.IsOpen))return;
        if(KeypadOpen && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))SubmitCode(digits);
        if(Input.GetKeyDown(KeyCode.Backspace)){if(KeypadOpen)CloseKeypad();else subtitleUntil=0;}
    }
    public void ShowSubtitle(string text,float seconds){Say(text,seconds);}
    void Say(string text,float seconds){subtitle=text;subtitleUntil=Time.time+seconds;}
    void OnGUI()
    {
        if(PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen)return;
        var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("UIFont"),fontSize=20,wordWrap=true,alignment=TextAnchor.MiddleCenter};
        if(Time.time<subtitleUntil){GUI.Box(new Rect(30,Screen.height-220,Screen.width-60,110),"");GUI.Label(new Rect(45,Screen.height-215,Screen.width-90,100),subtitle+"\n退格键跳过",style);}
        if(!KeypadOpen)return;
        float x=Screen.width/2-190,y=Screen.height/2-130;
        GUI.Box(new Rect(x,y,380,350),"");
        GUI.Label(new Rect(x+15,y+10,350,60),feedback,style);
        digits=GUI.TextField(new Rect(x+50,y+85,280,35),digits,3);
        if(GUI.Button(new Rect(x+45,y+145,140,45),"确认密码 (Enter)"))SubmitCode(digits);
        if(GUI.Button(new Rect(x+195,y+145,140,45),"关闭"))CloseKeypad();
        GUI.Label(new Rect(x+15,y+205,350,125),(State.paper?"已读试卷：按 2、1、3 排读取。":"试卷：尚未调查")+"\n"+(State.chart?"已读座位表：1 排 1 席；2 排 4 席；3 排 6 席。":"座位表：尚未调查"),style);
    }
}

using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CampusDepartureState
{
    public bool gateOpen, departed, completed;
}

/// <summary>Final campus gate, physical departure and existing motorcycle chapter ending.</summary>
public class CampusDepartureChapter : MonoBehaviour
{
    public Transform gate;
    public CampusEscapeObject console;
    Vector3 closedPosition;
    CampusFlowController flow;
    CampusEscapeDirector director;
    CampusExamChapter subtitles;
    float slideUntil;
    public CampusDepartureState State { get { return GameProgress.Ensure().departureChapter; } }
    public bool Prerequisites {
        get {
            var exam=GetComponent<CampusExamChapter>().State;
            var corridor=GetComponent<CampusCorridorChapter>().State;
            var library=GetComponent<CampusLibraryChapter>().State;
            return exam.paper && exam.chart && exam.key && exam.sideOpen && corridor.card && corridor.roomOpen && corridor.record && corridor.circuit && library.indexRead && library.recordRead && library.manual && library.key && library.utilityOpen && library.emergency;
        }
    }
    public string Objective { get { return !State.gateOpen?"前往庭院校门控制箱":!State.departed?"穿过已开启的校门":!State.completed?"骑上摩托车，沿校外道路驶离":"第一章完成 · 开始城市旅行"; } }
    public string Detail { get { return !State.gateOpen?"从庭院西侧通口进入校门前廊，控制箱按 E；需要完成教学楼全部关键机关":!State.departed?"走出校门后解锁骑乘；已开启的区域可以回访":!State.completed?"校门外左侧按 F 骑车，沿标有「城镇」的公路向前行驶":"回家留条、购买手机，再前往交通枢纽；M 查看地图"; } }
    public static void Build(Transform root)
    {
        var chapter=root.gameObject.AddComponent<CampusDepartureChapter>();
        chapter.gate=root.Find("EscapeObjectives/LockedCampusGate");
        chapter.console=root.Find("EscapeObjectives/GateRelease").GetComponent<CampusEscapeObject>();
        chapter.console.displayName="校门控制箱 · 核验紧急疏散权限";
        // The original trigger overlaps the inside of the locked gate. Departure needs an outside crossing.
        var trigger=GameObject.Find("SchoolGate");trigger.transform.position=new Vector3(-14,1,54.6f);
        trigger.GetComponent<BoxCollider>().size=new Vector3(4.6f,2.8f,1.2f);
        var sign=new GameObject("TownDepartureSign");sign.transform.SetParent(root,false);sign.transform.position=new Vector3(-12.8f,2,70);
        sign.transform.rotation=Quaternion.Euler(0,180,0);var text=sign.AddComponent<TextMesh>();
        text.font=Resources.Load<Font>("UIFont");text.text="城镇 ↑\n沿公路驶离校园";text.fontSize=40;text.characterSize=.07f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
        if(text.font!=null)sign.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
        sign.AddComponent<CampusWorldSign>();
    }
    void Start()
    {
        closedPosition=gate.position;flow=UnityCompat.FindOne<CampusFlowController>();director=GetComponent<CampusEscapeDirector>();subtitles=GetComponent<CampusExamChapter>();
        GameProgress.Ensure().SaveApplied+=Restore;Restore();
    }
    void OnDestroy(){if(GameProgress.Instance!=null)GameProgress.Instance.SaveApplied-=Restore;}
    public void Restore()
    {
        slideUntil=0;ApplyGate();gate.position=closedPosition+(State.gateOpen?Vector3.right*5:Vector3.zero);
        if(flow!=null)flow.RestoreDeparture(State.departed);
    }
    void ApplyGate()
    {
        gate.GetComponent<Collider>().enabled=!State.gateOpen;
        gate.GetComponent<NavMeshObstacle>().enabled=!State.gateOpen;
        director.RestoreGateProgress(State.gateOpen);
        console.displayName=State.gateOpen?"校门已开启":"校门控制箱 · 核验紧急疏散权限";
        var tint=console.GetComponent<PrimitiveTint>();var color=State.gateOpen?new Color(.2f,.8f,.4f):new Color(.85f,.5f,.16f);
        if(tint!=null)tint.color=color;
        console.GetComponent<Renderer>().material.color=color;
    }
    public bool UseGate()
    {
        if(!Prerequisites){subtitles.ShowSubtitle("疏散权限不足：先完成 201、203、图书馆和配电间的关键机关。",8);return true;}
        if(State.gateOpen)return true;
        State.gateOpen=true;ApplyGate();slideUntil=Time.time+.8f;
        subtitles.ShowSubtitle("广播：紧急门禁核验通过，校门开启。出口就在前面。",6);SaveSystem.Save();return true;
    }
    public bool TryDepart()
    {
        if(!State.gateOpen || !Prerequisites || flow==null || flow.player==null)return false;
        var at=flow.player.position;
        if(!State.departed && (at.z<54 || at.z>57 || at.x< -16.3f || at.x> -11.7f || at.y<0 || at.y>2))return false;
        if(!State.departed){State.departed=true;subtitles.ShowSubtitle("我真的出来了。先骑上门外的摩托车，沿公路离开这里。",7);}
        return true;
    }
    void Update()
    {
        if(slideUntil>0){float t=1-Mathf.Clamp01((slideUntil-Time.time)/.8f);gate.position=closedPosition+Vector3.right*5*Mathf.SmoothStep(0,1,t);if(t>=1)slideUntil=0;}
        if(State.completed || !State.departed || flow==null || flow.IsBusy || Time.timeScale==0)return;
        var bike=UnityCompat.FindOne<CampusMotorcycle>();
        if(bike==null || !bike.IsRiding || bike.SpeedKmh<5)return;
        var at=bike.transform.position;
        if(at.z<80 || at.z>100 || at.x< -12 || at.x> -4)return;
        State.completed=true;GameProgress.Ensure().SetCheckpoint(new Vector3(-14,.92f,81));
        flow.ShowTip("第一章 · 逃离考场 完成","校园已在身后。城市与旅行任务已开启。");
        subtitles.ShowSubtitle("路终于在我自己脚下了。先回家留张纸条，再决定去哪里。",7);
        SaveSystem.Save();Debug.Log("CAMPUS_DEPARTURE_COMPLETE");
    }
}

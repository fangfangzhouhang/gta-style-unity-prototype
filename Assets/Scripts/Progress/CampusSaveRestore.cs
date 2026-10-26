using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Restores a loaded campus only after all chapter components have restored their locks.</summary>
public class CampusSaveRestore : MonoBehaviour
{
    public bool Restored { get; private set; }
    IEnumerator Start()
    {
        var g=GameProgress.Ensure();var d=g.pendingCampusRestore;
        if(d==null || SceneManager.GetActiveScene().name!="CampusWorld")yield break;
        var flow=UnityCompat.FindOne<CampusFlowController>();var motor=flow.player.GetComponent<ThirdPersonController>();motor.controlEnabled=false;
        yield return null;yield return null;
        Apply(d);g.pendingCampusRestore=null;Restored=true;
    }
    public static void Capture(SaveData d)
    {
        if(d.sceneName!="CampusWorld")return;
        var session=UnityCompat.FindOne<CampusSaveRestore>();var flow=UnityCompat.FindOne<CampusFlowController>();
        if(session==null || flow==null || flow.player==null)return;
        if(flow.activeCheckpoint!=null){d.checkpointId=flow.activeCheckpoint.name;var p=flow.activeCheckpoint.position;d.checkpointX=p.x;d.checkpointY=p.y;d.checkpointZ=p.z;d.hasCheckpoint=true;}
        var bike=UnityCompat.FindOne<CampusMotorcycle>();
        d.hasPlayerPose=!flow.IsBusy && flow.player.GetComponent<CharacterController>().enabled && (bike==null || !bike.IsRiding);
        var at=flow.player.position;d.playerX=at.x;d.playerY=at.y;d.playerZ=at.z;var camera=UnityCompat.FindOne<ThirdPersonCamera>();d.playerYaw=camera!=null && camera.IsFirstPerson?camera.transform.eulerAngles.y:flow.player.eulerAngles.y;
    }
    public static bool CheckpointAllowed(string id)
    {
        var g=GameProgress.Ensure();
        if(id=="ExamCheckpoint")return true;
        if(id=="CorridorCheckpoint")return g.examChapter.sideOpen;
        if(id=="LibraryCheckpoint")return g.corridorChapter.circuit;
        if(id=="EastGroundCheckpoint" || id=="GroundSchoolCheckpoint")return g.libraryChapter.downOpen;
        if(id=="HubCheckpoint")return g.departureChapter.departed;
        if(id=="HomeCheckpoint")return g.departureChapter.completed;
        return false;
    }
    public static bool PositionAllowed(Vector3 at)
    {
        if(float.IsNaN(at.x)||float.IsNaN(at.y)||float.IsNaN(at.z)||float.IsInfinity(at.x)||float.IsInfinity(at.y)||float.IsInfinity(at.z))return false;
        var g=GameProgress.Ensure();
        if(at.x< -108 || at.x>128 || at.z< -158 || at.z>238 || at.y< -1 || at.y>12)return false;
        if(at.z>53.5f)return g.departureChapter.departed;
        if(at.y>7.7f)return g.corridorChapter.circuit && at.x> -18 && at.x<54 && at.z> -22 && at.z<22;
        if(at.y>3.7f)
        {
            if(at.x> -11.7f && at.x< -3 && at.z> -4 && at.z<8)return true;
            if(at.x>10 && at.x<20 && at.z> -4 && at.z<8 && !g.corridorChapter.roomOpen)return false;
            return g.examChapter.sideOpen && at.x> -18 && at.x<54 && at.z> -22 && at.z<32;
        }
        if(!g.libraryChapter.downOpen)return false;
        if(at.x> -2 && at.x<8 && at.z> -4 && at.z<8 && !g.libraryChapter.utilityOpen)return false;
        if(at.z>12.6f && !g.libraryChapter.emergency)return at.x< -11.7f && at.x> -17.2f && at.z<32;
        return true;
    }
    static bool ClearPose(Transform player,Vector3 at)
    {
        if(!PositionAllowed(at))return false;
        RaycastHit support;
        if(!Physics.Raycast(at+Vector3.up*.15f,Vector3.down,out support,1.5f,~0,QueryTriggerInteraction.Ignore) || support.normal.y<.7f || Mathf.Abs(support.point.y-(at.y-.9f))>.32f)return false;
        var feet=support.point;
        foreach(var col in Physics.OverlapCapsule(feet+Vector3.up*.36f,feet+Vector3.up*1.5f,.27f,~0,QueryTriggerInteraction.Ignore))
            if(!col.transform.IsChildOf(player))return false;
        return true;
    }
    public void Apply(SaveData d)
    {
        var g=GameProgress.Ensure();var flow=UnityCompat.FindOne<CampusFlowController>();
        var bike=UnityCompat.FindOne<CampusMotorcycle>();if(bike!=null)bike.ReleaseForRecovery();
        Transform cp=null;
        if(CheckpointAllowed(d.checkpointId??"")){var obj=GameObject.Find(d.checkpointId);if(obj!=null)cp=obj.transform;}
        if(cp==null && d.hasCheckpoint)
            foreach(string id in new[]{"ExamCheckpoint","CorridorCheckpoint","LibraryCheckpoint","EastGroundCheckpoint","GroundSchoolCheckpoint","HubCheckpoint","HomeCheckpoint"})
            {var obj=GameObject.Find(id);if(obj!=null && CheckpointAllowed(id) && Vector3.Distance(obj.transform.position,new Vector3(d.checkpointX,d.checkpointY,d.checkpointZ))<1.2f){cp=obj.transform;break;}}
        if(cp==null)
        {
            string id=g.departureChapter.departed?"HubCheckpoint":g.libraryChapter.emergency?"GroundSchoolCheckpoint":g.libraryChapter.manual?"LibraryCheckpoint":g.examChapter.sideOpen?"CorridorCheckpoint":"ExamCheckpoint";
            cp=GameObject.Find(id).transform;
        }
        var body=flow.player.GetComponent<CharacterController>();body.enabled=false;Physics.SyncTransforms();
        var at=new Vector3(d.playerX,d.playerY,d.playerZ);bool exact=d.hasPlayerPose && ClearPose(flow.player,at);
        flow.player.position=exact?at:cp.position;flow.player.rotation=Quaternion.Euler(0,exact && !float.IsNaN(d.playerYaw)&&!float.IsInfinity(d.playerYaw)?d.playerYaw:cp.eulerAngles.y,0);body.enabled=true;
        var motor=flow.player.GetComponent<ThirdPersonController>();motor.ResetVerticalSpeed();motor.controlEnabled=true;
        var pos=flow.player.position;
        var zone=g.departureChapter.departed && pos.z>53?CampusFlowController.Zone.Hub:g.examChapter.sideOpen?CampusFlowController.Zone.School:CampusFlowController.Zone.Exam;
        if(g.departureChapter.completed && pos.x> -34 && pos.x< -22 && pos.z>112 && pos.z<124)zone=CampusFlowController.Zone.Home;
        if(pos.x> -11.7f && pos.x< -3 && pos.y>4 && pos.y<7 && pos.z> -4 && pos.z<8)zone=CampusFlowController.Zone.Exam;
        flow.RestoreChapterSession(zone,cp);g.SetCheckpoint(cp.position);
        foreach(var teacher in UnityCompat.FindAll<TeacherAgent>())teacher.ReturnHome();SafeDiscoveryReset.BeginInvulnerability(3.5f);
        var camera=UnityCompat.FindOne<ThirdPersonCamera>();if(camera!=null)camera.RestoreLook(flow.player.eulerAngles.y);
        Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;
        Debug.Log("CAMPUS_SAVE_POSITION_RESTORED exact="+exact+" zone="+zone+" checkpoint="+cp.name);
    }
}

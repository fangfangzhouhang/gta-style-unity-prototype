using UnityEngine;

public class CampusEscapeObject : MonoBehaviour
{
    public enum Action { EvacuationPlan, MaintenanceKey, UtilityDoor, PowerSwitch, GateRelease, DiversionBell, ExamPaper, ExamChart, ExamCabinet, ExamSideDoor, ExamFrontDoor, ExamWindow }
    public Action action;
    public string displayName;
}

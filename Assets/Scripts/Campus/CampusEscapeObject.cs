using UnityEngine;

public class CampusEscapeObject : MonoBehaviour
{
    public enum Action { EvacuationPlan, MaintenanceKey, UtilityDoor, PowerSwitch, GateRelease, DiversionBell, ExamPaper, ExamChart, ExamCabinet, ExamSideDoor, ExamFrontDoor, ExamWindow, CorridorNotice, CorridorNPC, AccessCard, EquipmentDoor, EquipmentRecord, LightingCircuit, AlarmCircuit, DoorPowerCircuit, CircuitConfirm, EastAccessGate, CorridorBarrier }
    public Action action;
    public string displayName;
}

using UnityEngine;

public class CampusEscapeObject : MonoBehaviour
{
    public enum Action { EvacuationPlan, MaintenanceKey, UtilityDoor, PowerSwitch, GateRelease, DiversionBell, ExamPaper, ExamChart, ExamCabinet, ExamSideDoor, ExamFrontDoor, ExamWindow, CorridorNotice, CorridorNPC, AccessCard, EquipmentDoor, EquipmentRecord, LightingCircuit, AlarmCircuit, DoorPowerCircuit, CircuitConfirm, EastAccessGate, CorridorBarrier, LibraryIndex, LibraryRecord, LibraryArchive, LibraryWrongArchive, LibraryStairLock, LibraryShortcut, LibraryBarrier, ServiceExit }
    public Action action;
    public string displayName;
}

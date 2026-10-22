using UnityEngine;

public class CampusEscapeObject : MonoBehaviour
{
    public enum Action { EvacuationPlan, MaintenanceKey, UtilityDoor, PowerSwitch, GateRelease, DiversionBell }
    public Action action;
    public string displayName;
}

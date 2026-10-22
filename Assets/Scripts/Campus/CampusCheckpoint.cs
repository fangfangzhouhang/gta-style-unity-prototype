using UnityEngine;

public class CampusCheckpoint : MonoBehaviour
{
    public Transform checkpoint;
    void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<ThirdPersonController>() == null) return;
        var flow = UnityCompat.FindOne<CampusFlowController>();
        if (flow == null || flow.CurrentZone != CampusFlowController.Zone.School || checkpoint==null)return;
        var library=UnityCompat.FindOne<CampusLibraryChapter>();
        if(library!=null && checkpoint.name.Contains("Ground") && !library.State.downOpen)return;
        if(library!=null && checkpoint.name=="LibraryCheckpoint" && !library.GetComponent<CampusCorridorChapter>().State.circuit)return;
        flow.activeCheckpoint=checkpoint;
        GameProgress.Ensure().SetCheckpoint(checkpoint.position);
    }
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>Small, recoverable local patrols keep the centre of the gallery clear.</summary>
public class CampusCorridorNPC : MonoBehaviour
{
    public string characterName;
    public bool mainClue;
    NavMeshAgent agent;
    Vector3 home;
    float retryAt;
    bool right;
    public static void Build(Transform root,string name,Vector3 at,string label,bool clue)
    {
        var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.position=at;
        var body=LevelBuildUtil.Make(PrimitiveType.Capsule,go.transform,"StudentBody",new Vector3(0,.9f,0),new Vector3(.48f,.9f,.48f),Quaternion.identity,new Color(.23f,.4f,.6f),"Campus_Student",true);
        var item=go.AddComponent<CampusEscapeObject>();item.action=CampusEscapeObject.Action.CorridorNPC;item.displayName="与"+label+"交谈";
        var npc=go.AddComponent<CampusCorridorNPC>();npc.characterName=label;npc.mainClue=clue;
        var nav=go.AddComponent<NavMeshAgent>();nav.radius=.24f;nav.height=1.8f;nav.speed=.7f;nav.angularSpeed=150;nav.stoppingDistance=.15f;nav.avoidancePriority=65;
    }
    void Start(){agent=GetComponent<NavMeshAgent>();home=transform.position;retryAt=Time.time+2;}
    void Update()
    {
        if(agent==null || !agent.enabled || !agent.isOnNavMesh || Time.time<retryAt)return;
        // Reissue a nearby destination even after a temporary obstruction or partial path.
        right=!right;var goal=home+Vector3.right*(right?.65f:-.65f);
        if(NavMesh.SamplePosition(goal,out NavMeshHit hit,.8f,agent.areaMask))agent.SetDestination(hit.position);
        retryAt=Time.time+4;
    }
}

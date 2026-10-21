using UnityEngine;

public class HubTownBootstrap : MonoBehaviour
{
    void Awake()
    {
        GameProgress.Ensure();
        PauseMenu.Ensure();
        if (GameObject.Find("LevelRoot") != null || GameObject.Find("Player") != null)
            return;
        HubTownBuilder.Build();
    }

    void Start()
    {
        PrimitiveTint.ApplyAll();
    }
}

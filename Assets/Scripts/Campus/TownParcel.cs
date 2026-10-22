using UnityEngine;

/// <summary>Editable greybox lot metadata. Positions are game coordinates, not surveyed property boundaries.</summary>
public class TownParcel : MonoBehaviour
{
    public string parcelId, use, reference;
    public Vector2 footprint;
    public float plannedHeight;
}

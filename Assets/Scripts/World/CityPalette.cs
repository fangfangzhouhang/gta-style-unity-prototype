using UnityEngine;

/// <summary>
/// 占位几何体的颜色。场景材质与运行时上色都用这一套。
/// </summary>
public static class CityPalette
{
    public static readonly Color Ground = new Color(0.36f, 0.55f, 0.31f, 1f);
    public static readonly Color Road = new Color(0.18f, 0.19f, 0.21f, 1f);
    public static readonly Color Sidewalk = new Color(0.55f, 0.55f, 0.53f, 1f);
    public static readonly Color LaneMark = new Color(0.92f, 0.78f, 0.22f, 1f);
    public static readonly Color BuildingA = new Color(0.75f, 0.68f, 0.58f, 1f);
    public static readonly Color BuildingB = new Color(0.52f, 0.60f, 0.68f, 1f);
    public static readonly Color BuildingC = new Color(0.62f, 0.42f, 0.36f, 1f);
    public static readonly Color Glass = new Color(0.22f, 0.30f, 0.38f, 1f);
    public static readonly Color Player = new Color(0.22f, 0.48f, 0.82f, 1f);
    public static readonly Color CarBody = new Color(0.78f, 0.16f, 0.14f, 1f);
    public static readonly Color CarCabin = new Color(0.14f, 0.18f, 0.24f, 1f);
    public static readonly Color Wheel = new Color(0.08f, 0.08f, 0.09f, 1f);
    public static readonly Color Checkpoint = new Color(1f, 0.82f, 0.12f, 1f);
    public static readonly Color Headlight = new Color(1f, 0.96f, 0.75f, 1f);
    public static readonly Color Crate = new Color(0.45f, 0.32f, 0.18f, 1f);
    public static readonly Color Lamp = new Color(0.25f, 0.25f, 0.27f, 1f);

    public static Color Building(int variant)
    {
        switch (variant)
        {
            case 1: return BuildingB;
            case 2: return BuildingC;
            default: return BuildingA;
        }
    }

    public static string BuildingMaterial(int variant)
    {
        switch (variant)
        {
            case 1: return "BuildingB";
            case 2: return "BuildingC";
            default: return "BuildingA";
        }
    }
}

using UnityEngine;

/// <summary>
/// 给方块上色。Play 时按当前渲染管线挑选可用 Shader，
/// 这样内置管线用 Standard，URP/HDRP 也不会一直是粉色。
/// </summary>
public class PrimitiveTint : MonoBehaviour
{
    public Color color = Color.white;
    [Range(0f, 1f)] public float smoothness = 0.18f;
    public bool emission;
    public string materialName;

    public void Apply(Shader shader)
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null || shader == null)
            return;

        var mat = new Material(shader);
        mat.name = string.IsNullOrEmpty(materialName) ? gameObject.name : materialName;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0f);

        if (emission)
        {
            Color glow = color * 1.6f;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", glow);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        renderer.sharedMaterial = mat;
    }

    public static void ApplyAll()
    {
        if (!Application.isPlaying)
            return;

        Shader shader = FindLitShader();
        if (shader == null)
            return;

        var all = UnityCompat.FindAll<PrimitiveTint>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
                all[i].Apply(shader);
        }
    }

    public static Shader FindLitShader()
    {
        string[] names =
        {
            "Universal Render Pipeline/Lit",
            "HDRP/Lit",
            "Standard",
            "Diffuse",
            "Legacy Shaders/Diffuse",
            "Unlit/Color",
            "Sprites/Default"
        };

        for (int i = 0; i < names.Length; i++)
        {
            Shader shader = Shader.Find(names[i]);
            if (shader != null)
                return shader;
        }

        return null;
    }
}

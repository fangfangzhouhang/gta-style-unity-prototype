using UnityEngine;

/// <summary>Physical building labels respect wall depth, including dynamic font atlas changes.</summary>
[ExecuteAlways]
[RequireComponent(typeof(TextMesh))]
public class CampusWorldSign : MonoBehaviour
{
    Material material;
    void OnWillRenderObject()
    {
        var text=GetComponent<TextMesh>();if(text.font==null)return;
        if(material==null)
        {
            var shader=Shader.Find("Campus/WorldText");if(shader==null)return;
            material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
        }
        material.mainTexture=text.font.material.mainTexture;
        GetComponent<Renderer>().sharedMaterial=material;
    }
    void OnDisable(){if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);material=null;}}
}

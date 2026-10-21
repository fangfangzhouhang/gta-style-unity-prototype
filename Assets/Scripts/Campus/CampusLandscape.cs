using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>米制枝干、单片叶面、草簇与共享 PBR 材质；细节网格按距离切换。</summary>
public static class CampusLandscape
{
    class Geometry
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector2> uv = new List<Vector2>();
        public readonly List<Color> colors = new List<Color>();
        public readonly List<int> triangles = new List<int>();
        public void Tube(Vector3 a, Vector3 b, float radius, float tip)
        {
            int start = vertices.Count;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, (b-a).normalized);
            for (int ring = 0; ring < 2; ring++) for (int i = 0; i <= 12; i++)
            {
                float angle = i * Mathf.PI * 2 / 12;
                float r = ring == 0 ? radius : tip;
                vertices.Add((ring == 0 ? a : b) + rotation * new Vector3(Mathf.Cos(angle)*r, 0, Mathf.Sin(angle)*r));
                uv.Add(new Vector2(i / 12f, ring * Vector3.Distance(a,b)));
                colors.Add(Color.white);
            }
            for (int i = 0; i < 12; i++)
            {
                int n = start+i;
                triangles.AddRange(new[] { n,n+13,n+1,n+1,n+13,n+14 });
            }
        }
        public void Leaf(Vector3 center, Quaternion rotation, float length, Color color)
        {
            int start = vertices.Count;
            Vector3[] shape = { new Vector3(0,0,-0.5f), new Vector3(-0.3f,0,0), new Vector3(0,0.07f,0), new Vector3(0.3f,0,0), new Vector3(0,0,0.5f) };
            foreach (var v in shape) { vertices.Add(center + rotation * (v*length)); uv.Add(new Vector2(v.x+0.5f,v.z+0.5f)); colors.Add(color); }
            foreach (int index in new[] {0,1,2,0,2,3,1,4,2,2,4,3}) triangles.Add(start+index);
        }
        public Mesh Mesh(string name)
        {
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetUVs(0,uv); mesh.SetColors(colors); mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
    static readonly Dictionary<int, Mesh[]> trees = new Dictionary<int, Mesh[]>();
    static Material bark, leaves, grass;
    static float R(System.Random random, float a, float b) { return Mathf.Lerp(a,b,(float)random.NextDouble()); }

    public static Material Grass
    {
        get
        {
            if (grass == null)
            {
                grass = new Material(Shader.Find("Standard")) { name="CampusArt_Grass", color=Color.white };
                grass.mainTexture = Resources.Load<Texture2D>("CampusArt/leafy_grass_diff");
                grass.mainTextureScale = new Vector2(90,90);
                SetNormal(grass,"leafy_grass_nor_gl");
                grass.SetFloat("_Glossiness",0.08f);
            }
            return grass;
        }
    }
    static void SetNormal(Material material, string path)
    {
        var texture = Resources.Load<Texture2D>("CampusArt/"+path);
        if (texture != null) { material.SetTexture("_BumpMap",texture); material.EnableKeyword("_NORMALMAP"); material.SetFloat("_BumpScale",0.5f); }
    }
    static void Materials()
    {
        if (bark == null)
        {
            bark = new Material(Shader.Find("Standard")) { name="CampusArt_Bark", color=Color.white };
            bark.mainTexture = Resources.Load<Texture2D>("CampusArt/bark_brown_02_diff");
            SetNormal(bark,"bark_brown_02_nor_gl"); bark.SetFloat("_Glossiness",0.12f);
        }
        if (leaves == null) leaves = new Material(Shader.Find("Campus/Foliage")) { name="CampusArt_Leaves" };
    }
    static GameObject Surface(Transform root,string name,Mesh mesh,Material material)
    {
        var go = new GameObject(name); go.transform.SetParent(root,false);
        go.AddComponent<MeshFilter>().sharedMesh=mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial=material;
        return go;
    }

    public static void Tree(Transform parent,Vector3 position,int variant,float scale=1f)
    {
        Materials();
        if (!trees.ContainsKey(variant))
        {
            var random = new System.Random(1200+variant);
            var wood = new Geometry(); var foliage = new Geometry(); var low = new Geometry();
            wood.Tube(Vector3.zero,new Vector3(0.08f,3.3f,0),0.19f,0.075f);
            for (int branch=0;branch<20;branch++)
            {
                float angle=branch*2.39996f;
                float y=R(random,2.2f,4.1f);
                Vector3 start=new Vector3(0,y*0.7f,0);
                Vector3 tip=new Vector3(Mathf.Cos(angle)*R(random,0.8f,1.8f),y,Mathf.Sin(angle)*R(random,0.8f,1.8f));
                wood.Tube(start,tip,0.045f,0.009f);
                for (int leaf=0;leaf<140;leaf++)
                {
                    Vector3 offset=new Vector3(R(random,-0.85f,0.85f),R(random,-0.55f,0.65f),R(random,-0.85f,0.85f));
                    if (offset.sqrMagnitude>0.8f) continue;
                    Quaternion rotation=Quaternion.Euler(R(random,-65,65),R(random,0,360),R(random,-35,35));
                    Color color=Color.Lerp(new Color(0.45f,0.6f,0.18f),new Color(1f,1f,0.68f),R(random,0,1));
                    foliage.Leaf(tip+offset,rotation,R(random,0.17f,0.28f),color);
                    if (leaf%3==0) low.Leaf(tip+offset,rotation,0.34f,color);
                }
            }
            trees[variant]=new[] {wood.Mesh("CampusArt_Wood_"+variant),foliage.Mesh("CampusArt_Canopy_"+variant),low.Mesh("CampusArt_CanopyLow_"+variant)};
        }
        var tree=new GameObject("CampusTree"); tree.transform.SetParent(parent,false); tree.transform.localPosition=position;
        tree.transform.localScale=Vector3.one*scale;
        var trunk=Surface(tree.transform,"Bark",trees[variant][0],bark);
        var high=Surface(tree.transform,"Leaves",trees[variant][1],leaves);
        var lowGo=Surface(tree.transform,"Leaves_LOD1",trees[variant][2],leaves);
        var collider=tree.AddComponent<CapsuleCollider>(); collider.center=new Vector3(0,1.5f,0); collider.height=3f; collider.radius=0.2f;
        var lod=tree.AddComponent<LODGroup>();
        lod.SetLODs(new[] {new LOD(0.08f,new[] {trunk.GetComponent<Renderer>(),high.GetComponent<Renderer>()}),
            new LOD(0.012f,new[] {trunk.GetComponent<Renderer>(),lowGo.GetComponent<Renderer>()})});
        lod.RecalculateBounds();
    }

    public static void PlantBed(Transform parent,Vector3 position,int seed)
    {
        Materials();
        var random=new System.Random(seed); var mesh=new Geometry();
        for (int plant=0;plant<110;plant++)
        {
            Vector3 center=position+new Vector3(R(random,-2,2),0,R(random,-1.2f,1.2f));
            for (int leaf=0;leaf<6;leaf++)
            {
                float y=R(random,0.08f,0.42f);
                mesh.Leaf(center+new Vector3(R(random,-0.2f,0.2f),y,R(random,-0.2f,0.2f)),
                    Quaternion.Euler(R(random,25,80),R(random,0,360),0),R(random,0.18f,0.4f),new Color(R(random,0.5f,0.9f),R(random,0.7f,1f),0.4f));
            }
        }
        Surface(parent,"CampusPlantBed",mesh.Mesh("CampusArt_PlantBed_"+seed),leaves);
    }
}

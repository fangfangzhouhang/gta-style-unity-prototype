using System.Collections.Generic;
using UnityEngine;

/// <summary>Metre-sized campus lots, one terrain surface, and separated town houses.</summary>
public static class CampusSitePlan
{
    static readonly Color Brick = new Color(.58f,.23f,.15f);
    static readonly Color Yellow = new Color(.86f,.73f,.46f);
    static readonly Color Stone = new Color(.59f,.58f,.52f);
    static readonly Color Glass = new Color(.22f,.36f,.41f);
    static GameObject Box(Transform p,string n,Vector3 v,Vector3 s,Color c,bool solid=true)
    { return LevelBuildUtil.Make(PrimitiveType.Cube,p,n,v,s,Quaternion.identity,c,"Campus_"+n,solid); }
    static void Remove(string name) { var go=GameObject.Find(name); if(go!=null) LevelBuildUtil.DestroyObject(go); }

    // The built-up area remains level. The outskirts rise smoothly, without steps
    // under the existing escape route or town interaction/checkpoint positions.
    public static float Height(float x,float z)
    {
        float east=Mathf.SmoothStep(0,1,Mathf.InverseLerp(80,128,x));
        float west=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-72,-108,x));
        float north=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-120,-158,z));
        float south=Mathf.SmoothStep(0,1,Mathf.InverseLerp(232,238,z));
        if(z>54 && z<225)
        {
            east=Mathf.SmoothStep(0,1,Mathf.InverseLerp(113,128,x));
            west=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-100,-108,x));
        }
        return -.12f + Mathf.Max(east,west,north,south) *
            (1.7f + 1.1f*Mathf.Sin(x*.052f+z*.034f)*Mathf.Sin(z*.043f));
    }
    public static void Build(Transform root)
    {
        foreach(string name in new[]{"TownGround","WestGardenGround","LinkPad","BldgA","BldgB","BldgC","GateVisual"}) Remove(name);
        var site=new GameObject("CampusSite_160x167m").transform; site.SetParent(root,false);
        Terrain(site);
        // A single raised apron replaces two coincident grass slabs.
        var apron=GameObject.Find("YardFloor");
        apron.transform.position=new Vector3(-10,-.08f,59.1f);
        apron.transform.localScale=new Vector3(28,.2f,13.8f);
        // Intersecting garden paths have a deliberate, small height separation.
        var link=GameObject.Find("GardenLink"); link.transform.position+=Vector3.up*.025f;
        var loop=GameObject.Find("GardenLoop"); loop.transform.position+=Vector3.up*.045f;

        // Existing playable exam walls remain the west end of the academic block.
        var exam=GameObject.Find("ExamHall");
        foreach(var r in exam.GetComponentsInChildren<Renderer>())
            if(r.name.StartsWith("Wall") || r.name.Contains("SillWall") || r.name.Contains("LintelWall") || r.name=="WindowPier")
            { var tint=r.GetComponent<PrimitiveTint>(); if(tint!=null) LevelBuildUtil.DestroyObject(tint);
              var mat=new Material(Shader.Find("Standard")){name="CampusArt_ExamOchre_v10",color=Yellow}; r.sharedMaterial=mat; }
        var school=GameObject.Find("SchoolBuilding");
        foreach(var r in school.GetComponentsInChildren<Renderer>())
            if(r.name.StartsWith("CorrWall") || r.name.StartsWith("UpperLeft") || r.name.StartsWith("UpperRight") || r.name.StartsWith("UpperBack") || r.name.StartsWith("StairEnclosure"))
            {var tint=r.GetComponent<PrimitiveTint>();if(tint!=null)LevelBuildUtil.DestroyObject(tint);
             r.sharedMaterial=new Material(Shader.Find("Standard")){name="CampusArt_CorridorOchre_v10",color=Yellow};}
        Box(site,"CorridorRedPlinth",new Vector3(-19.23f,.55f,24),new Vector3(.08f,1.1f,56),Brick,false);
        Box(site,"CorridorRoofTrim",new Vector3(-14,4.06f,42),new Vector3(10.2f,.10f,20.2f),Brick);
        Box(site,"UpperRoofTrim",new Vector3(-14.4f,7.67f,9),new Vector3(5.6f,.12f,26.4f),Brick);
        CampusTeachingBuilding.Build(root);
        Box(site,"ExamRoofCornice",new Vector3(-7.5f,7.76f,2),new Vector3(9.6f,.18f,12.6f),Brick);
        Box(site,"ExamRedSillBand",new Vector3(-2.86f,4.95f,2),new Vector3(.08f,.28f,12.2f),Brick,false);
        Building(site,"Library",new Vector3(62,0,-51),new Vector3(28,11.4f,18),"图书馆");
        Building(site,"DiningHall",new Vector3(58,0,-93),new Vector3(32,7.6f,18),"学生食堂");
        Building(site,"Gymnasium",new Vector3(-42,0,-96),new Vector3(40,8,22),"体育馆");
        Box(site,"CampusCourtyard",new Vector3(25,-.025f,29),new Vector3(58,.15f,28),new Color(.70f,.67f,.57f));
        Box(site,"CampusSpine",new Vector3(37,-.025f,-56),new Vector3(7,.15f,126),Stone);
        Box(site,"WestCampusWalk",new Vector3(-24,-.025f,-52),new Vector3(5,.15f,108),Stone);
        Box(site,"NorthCampusWalk",new Vector3(5,-.025f,-111),new Vector3(142,.15f,4),Stone);
        Box(site,"TeachingLinkWalk",new Vector3(9,-.005f,-36),new Vector3(60,.11f,4),Stone);
        Track(site);
        Basketball(site,new Vector3(63,0,-18));
        // Boundary is 160 x 167 metres (2.67 hectares), with an open six-metre gate.
        Fence(site,new Vector3(-75,0,-31.5f),167,true);
        Fence(site,new Vector3(85,0,-31.5f),167,true);
        Fence(site,new Vector3(5,0,-115),160,false);
        Fence(site,new Vector3(-47,0,52),56,false);
        Fence(site,new Vector3(38,0,52),94,false);
        Box(site,"SchoolGateHeader",new Vector3(-14,4.15f,52.5f),new Vector3(7.4f,.8f,1.1f),Brick);
        Box(site,"GateReturnLeft",new Vector3(-18.2f,1.8f,52.25f),new Vector3(1.6f,3.6f,.9f),Yellow);
        Box(site,"GateReturnRight",new Vector3(-9.8f,1.8f,52.25f),new Vector3(1.6f,3.6f,.9f),Yellow);
        Sign(site,"校园 · 安全出口",new Vector3(-14,4.14f,53.08f),.27f);
        Building(site,"Gatehouse",new Vector3(-27,0,45),new Vector3(6,3.5f,6),"门卫室");
        // Front-road parking bay, clear of the escape trigger and tree trunks.
        Box(site,"MotorcycleParking",new Vector3(-19.5f,.035f,59.5f),new Vector3(4,.07f,5),Stone);
        Box(site,"ParkingLink",new Vector3(-14.5f,.045f,60),new Vector3(6,.07f,2.5f),Stone);
        for(int i=0;i<7;i++)
        {
            CampusLandscape.Tree(site,new Vector3(3+i*10,0,42),i%3,1.1f);
            CampusLandscape.Tree(site,new Vector3(-67,0,-100+i*22),i%3,1.15f);
        }
        for(int i=0;i<5;i++) CampusLandscape.Tree(site,new Vector3(45,0,-41-i*14),i%3,1);
        // Three individual houses, all on independent lots. No imported building
        // is layered over these shells; spacing is checked in the scene validator.
        HangzhouTownBlockout.Build(root);
    }
    static void Terrain(Transform parent)
    {
        const int nx=121,nz=201;
        var v=new Vector3[nx*nz]; var uv=new Vector2[v.Length]; var triangles=new int[(nx-1)*(nz-1)*6];
        for(int z=0;z<nz;z++) for(int x=0;x<nx;x++)
        { int i=z*nx+x; float px=-110+2*x,pz=-160+2*z; v[i]=new Vector3(px,Height(px,pz),pz); uv[i]=new Vector2(px/2,pz/2); }
        int t=0;
        for(int z=0;z<nz-1;z++) for(int x=0;x<nx-1;x++)
        { int i=z*nx+x; triangles[t++]=i;triangles[t++]=i+nx;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+nx;triangles[t++]=i+nx+1; }
        var mesh=new Mesh{name="CampusArt_UnifiedTerrain_v11"}; mesh.vertices=v;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var go=new GameObject("TownGround",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(parent,false);
        go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;
        var grass=new Material(CampusLandscape.Grass){name="CampusArt_TerrainGrass_v10b"};
        grass.mainTextureScale=Vector2.one;
        go.GetComponent<MeshRenderer>().sharedMaterial=grass;
    }
    static void Building(Transform p,string name,Vector3 at,Vector3 size,string label,bool yellow=false)
    {
        var lot=new GameObject(name).transform;lot.SetParent(p,false);lot.localPosition=at;
        Color wall=yellow?Yellow:Brick;
        Box(lot,"BuildingBody",Vector3.up*size.y*.5f,size,wall);
        Box(lot,"StonePlinth",new Vector3(0,.3f,0),new Vector3(size.x+.1f,.6f,size.z+.1f),Stone);
        Box(lot,"RoofCornice",new Vector3(0,size.y+.13f,0),new Vector3(size.x+.6f,.26f,size.z+.6f),Yellow);
        int floors=Mathf.Max(1,Mathf.RoundToInt(size.y/3.8f));
        for(int floor=0;floor<floors;floor++)
        {
            float y=1.9f+floor*3.8f;
            foreach(int side in new[]{-1,1})
            {
                Box(lot,"OchreFloorBand",new Vector3(0,floor*3.8f+.85f,side*(size.z/2+.065f)),new Vector3(size.x,.3f,.12f),Yellow,false);
                for(float x=-size.x/2+2;x<size.x/2-1;x+=3.5f)
                {
                    var a=new Vector3(x,y,side*(size.z/2+.13f));
                    Box(lot,"WindowFrame",a,new Vector3(2.3f,1.9f,.14f),Yellow,false);
                    Box(lot,"WindowGlass",a+Vector3.forward*side*.085f,new Vector3(2.08f,1.65f,.025f),Glass,false);
                    Box(lot,"WindowMullion",a+Vector3.forward*side*.11f,new Vector3(.055f,1.67f,.04f),Stone,false);
                }
            }
        }
        foreach(int side in new[]{-1,1})
            for(float z=-size.z/2+2;z<size.z/2-1;z+=3.5f)
                for(int floor=0;floor<floors;floor++)
                {var a=new Vector3(side*(size.x/2+.08f),1.9f+floor*3.8f,z);
                 Box(lot,"SideWindowFrame",a,new Vector3(.14f,1.9f,2.3f),Yellow,false);
                 Box(lot,"SideGlass",a+Vector3.right*side*.085f,new Vector3(.025f,1.65f,2.08f),Glass,false);}
        if(label.Length>0) Sign(lot,label,new Vector3(0,size.y-.45f,size.z/2+.24f),.3f);
    }
    static void Sign(Transform p,string label,Vector3 at,float scale)
    {
        var go=new GameObject("CampusSign");go.transform.SetParent(p,false);go.transform.localPosition=at;go.transform.localRotation=Quaternion.Euler(0,180,0);
        var text=go.AddComponent<TextMesh>();text.text=label;text.characterSize=scale*.4f;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(1,.91f,.62f);
        var font=Resources.Load<Font>("UIFont");if(font!=null){text.font=font;go.GetComponent<MeshRenderer>().sharedMaterial=font.material;}
        go.AddComponent<CampusWorldSign>();
    }
    static void Fence(Transform p,Vector3 at,float length,bool alongZ)
    {
        Vector3 along=alongZ?Vector3.forward:Vector3.right;
        Box(p,"CampusBoundary",at+Vector3.up*.55f,alongZ?new Vector3(.32f,1.1f,length):new Vector3(length,1.1f,.32f),Brick);
        Box(p,"FenceRail",at+Vector3.up*1.85f,alongZ?new Vector3(.08f,.08f,length):new Vector3(length,.08f,.08f),Stone);
        for(float d=-length/2;d<=length/2;d+=2.5f)
            Box(p,"FencePillar",at+along*d+Vector3.up*1.1f,new Vector3(.38f,2.2f,.38f),Yellow);
        // Full-height collision, invisible above the masonry base.
        var barrier=new GameObject("FenceCollision");barrier.transform.SetParent(p,false);barrier.transform.localPosition=at+Vector3.up*1.1f;
        barrier.AddComponent<BoxCollider>().size=alongZ?new Vector3(.32f,2.2f,length):new Vector3(length,2.2f,.32f);
    }
    static void Track(Transform p)
    {
        var v=new List<Vector3>();var tri=new List<int>();
        for(int i=0;i<=128;i++)
        { float a=i*Mathf.PI*2/128;v.Add(new Vector3(-44+26*Mathf.Cos(a),.035f,-40+34*Mathf.Sin(a)));v.Add(new Vector3(-44+21*Mathf.Cos(a),.035f,-40+29*Mathf.Sin(a)));
          if(i<128){int n=i*2;tri.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});} }
        var mesh=new Mesh{name="CampusArt_Track_v10b"};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var track=new GameObject("SchoolRunningTrack",typeof(MeshFilter),typeof(MeshRenderer));track.transform.SetParent(p,false);track.GetComponent<MeshFilter>().sharedMesh=mesh;
        track.GetComponent<MeshRenderer>().sharedMaterial=new Material(Shader.Find("Standard")){name="CampusArt_TrackRed_v10",color=new Color(.54f,.22f,.16f)};
        Box(p,"SportsField",new Vector3(-44,-.02f,-40),new Vector3(24,.12f,40),new Color(.23f,.4f,.24f));
        foreach(float z in new[]{-60f,-20f})
        {
            foreach(float x in new[]{-47f,-41f})Box(p,"GoalPost",new Vector3(x,1.2f,z),new Vector3(.1f,2.4f,.1f),Color.white);
            Box(p,"GoalBar",new Vector3(-44,2.4f,z),new Vector3(6,.1f,.1f),Color.white);
        }
    }
    static void Basketball(Transform p,Vector3 at)
    {
        Box(p,"BasketballCourt_15x28m",at+Vector3.up*.01f,new Vector3(15,.12f,28),new Color(.30f,.44f,.46f));
        foreach(float x in new[]{-7.4f,7.4f}) Box(p,"CourtSideline",at+new Vector3(x,.078f,0),new Vector3(.06f,.01f,27.8f),Color.white,false);
        foreach(float z in new[]{-13.9f,0,13.9f}) Box(p,"CourtLine",at+new Vector3(0,.078f,z),new Vector3(14.8f,.01f,.06f),Color.white,false);
        foreach(float z in new[]{-12,12})
        { Box(p,"BasketPost",at+new Vector3(0,1.6f,z),new Vector3(.15f,3.2f,.15f),Stone);
          Box(p,"Backboard",at+new Vector3(0,3.25f,z),new Vector3(1.8f,1.05f,.06f),Color.white); }
    }
}

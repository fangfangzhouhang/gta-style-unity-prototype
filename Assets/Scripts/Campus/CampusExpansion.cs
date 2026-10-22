using UnityEngine;

public static class CampusExpansion
{
    static GameObject Part(Transform parent,string name,Vector3 position,Vector3 size,Color color,bool solid=true)
    {
        return LevelBuildUtil.Make(PrimitiveType.Cube,parent,name,position,size,Quaternion.identity,color,"Campus_"+name,solid);
    }
    public static void Build(Transform root)
    {
        var park=new GameObject("ExpandedCampusPark").transform;
        park.SetParent(root,false);
        // 主干路两侧的人行道，绿化与主线移动区域分开。
        Part(park,"TownSidewalkL",new Vector3(-14,0.04f,165),new Vector3(4,0.08f,100),new Color(0.65f,0.64f,0.59f));
        Part(park,"TownSidewalkR",new Vector3(-2,0.04f,165),new Vector3(4,0.08f,100),new Color(0.65f,0.64f,0.59f));
        Part(park,"GardenWalk",new Vector3(-96,0.02f,163),new Vector3(18,0.04f,3),new Color(0.67f,0.62f,0.51f));
        Part(park,"GardenLink",new Vector3(-96,0.02f,145),new Vector3(3,0.04f,40),new Color(0.67f,0.62f,0.51f));
        Part(park,"GardenLoop",new Vector3(-96,0.02f,190),new Vector3(3,0.04f,45),new Color(0.67f,0.62f,0.51f));
        for (int i=0;i<13;i++)
        {
            float z=63+i*11;
            CampusLandscape.Tree(park,new Vector3(-18.5f,0,z),i%3,1f+(i%3)*0.12f);
            CampusLandscape.Tree(park,new Vector3(1.5f,0,z+2),i%3,1.08f);
        }
        for (int i=0;i<8;i++)
        {
            float x=-103+(i%4)*4;
            float z=145+(i/4)*43;
            CampusLandscape.Tree(park,new Vector3(x,0,z),i%3,1.2f);
            Bed(park,new Vector3(x,0,z+5),400+i);
            Bench(park,new Vector3(x,0,160+(i/4)*22));
        }
        for (int i=0;i<5;i++)
        {
            CampusLandscape.Tree(park,new Vector3(-36-i*5,0,55),i%3,1f);
            Bed(park,new Vector3(-34-i*6,0,61),500+i);
        }
        // 可进入的街区道路与有窗、屋檐、入口尺度的楼体。
        Part(park,"NeighborhoodWalk",new Vector3(39,0.02f,158),new Vector3(78,0.04f,4),new Color(0.65f,0.64f,0.59f));
        for (int i=0;i<4;i++) Bench(park,new Vector3(12+i*16,0,155));
        Part(park,"WestGardenConnection",new Vector3(-26,0.02f,63),new Vector3(18,0.04f,4),new Color(0.65f,0.64f,0.59f));
    }
    static void Bed(Transform parent,Vector3 position,int seed)
    {
        Part(parent,"PlantBedSoil",position+Vector3.up*0.12f,new Vector3(4.2f,0.24f,2.6f),new Color(0.18f,0.14f,0.09f));
        Part(parent,"PlanterEdgeFront",position+new Vector3(0,0.2f,-1.35f),new Vector3(4.5f,0.4f,0.12f),new Color(0.53f,0.52f,0.47f));
        Part(parent,"PlanterEdgeBack",position+new Vector3(0,0.2f,1.35f),new Vector3(4.5f,0.4f,0.12f),new Color(0.53f,0.52f,0.47f));
        CampusLandscape.PlantBed(parent,position+Vector3.up*0.24f,seed);
    }
    static void Bench(Transform parent,Vector3 position)
    {
        for (int i=0;i<5;i++) Part(parent,"BenchSlat",position+new Vector3(0,0.46f,-0.24f+i*0.12f),new Vector3(1.8f,0.045f,0.095f),new Color(0.45f,0.29f,0.15f));
        for (int i=0;i<3;i++) Part(parent,"BenchBack",position+new Vector3(0,0.72f+i*0.12f,0.3f),new Vector3(1.8f,0.09f,0.04f),new Color(0.45f,0.29f,0.15f));
        foreach (float x in new[] {-0.65f,0.65f})
            Part(parent,"BenchLeg",position+new Vector3(x,0.23f,0),new Vector3(0.08f,0.46f,0.55f),new Color(0.18f,0.2f,0.19f));
    }
}

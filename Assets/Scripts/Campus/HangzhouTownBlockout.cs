using System;
using UnityEngine;

/// <summary>Fengqi Road-inspired massing: shop-lined avenue, residential lanes and a metro forecourt.</summary>
public static class HangzhouTownBlockout
{
    [Serializable] public class Lot { public string id,use;public float x,z,width,depth,height;public int roof; }
    [Serializable] public class Plan {public string referenceSchool,note;public Lot[] lots;}
    static GameObject Box(Transform p,string name,Vector3 at,Vector3 size,Color color,bool solid=true)
    {return LevelBuildUtil.Make(PrimitiveType.Cube,p,name,at,size,Quaternion.identity,color,"Town_"+name,solid);}
    public static void Build(Transform root)
    {
        var district=new GameObject("Hangzhou_FengqiRoad_Blockout").transform;district.SetParent(root,false);
        var text=Resources.Load<TextAsset>("CampusTown/HangzhouParcels");
        if(text==null)throw new InvalidOperationException("HangzhouParcels.json missing");
        var plan=JsonUtility.FromJson<Plan>(text.text);
        foreach(var lot in plan.lots)Building(district,lot);
        // Higher, continuous crossing surfaces avoid coplanar overlap with the old lane segments.
        Box(district,"FengqiRoad_ReferenceAvenue",new Vector3(9,.08f,80),new Vector3(230,.08f,14),new Color(.2f,.22f,.24f));
        Box(district,"ZhongheRoad_ReferenceWest",new Vector3(-81,.05f,154),new Vector3(10,.08f,140),new Color(.23f,.24f,.25f));
        // Aerial reference and local road reports both place the elevated road by
        // the school's west side. This is game-scale massing, not surveyed geometry.
        Box(district,"ZhongheElevatedDeck",new Vector3(-81,5.8f,65),new Vector3(10,.8f,336),new Color(.42f,.45f,.46f));
        foreach(float x in new[]{-85.7f,-76.3f})
            Box(district,"ViaductParapet",new Vector3(x,6.65f,65),new Vector3(.3f,.9f,336),new Color(.58f,.59f,.57f));
        for(int z=-96;z<=216;z+=24)
        {
            Box(district,"ViaductPier",new Vector3(-81,2.7f,z),new Vector3(1.2f,5.4f,1.2f),new Color(.56f,.57f,.54f));
            Box(district,"ViaductPierCap",new Vector3(-81,5.1f,z),new Vector3(7,.55f,1.5f),new Color(.5f,.52f,.51f));
        }
        Box(district,"XinhuaRoad_ReferenceEast",new Vector3(103,.05f,154),new Vector3(10,.08f,140),new Color(.23f,.24f,.25f));
        Box(district,"NeighborhoodCrossLane",new Vector3(10,.06f,155),new Vector3(176,.08f,7),new Color(.28f,.29f,.29f));
        Box(district,"TownNorthLane",new Vector3(10,.065f,227),new Vector3(196,.08f,7),new Color(.28f,.29f,.29f));
        foreach(float z in new[]{70,90})Box(district,"AvenueSidewalk",new Vector3(9,.035f,z),new Vector3(230,.07f,4),new Color(.61f,.61f,.57f));
        for(int i=0;i<38;i++)Box(district,"AvenueLaneDash",new Vector3(-101+i*6,.127f,80),new Vector3(3,.01f,.12f),new Color(.91f,.82f,.36f),false);
        foreach(float x in new[]{-81,103})for(int i=0;i<8;i++)
            Box(district,"PedestrianCrossing",new Vector3(x-3.4f+i,.132f,80),new Vector3(.55f,.01f,11),Color.white,false);
        Box(district,"MetroForecourt",new Vector3(6,.04f,115),new Vector3(16,.08f,18),new Color(.58f,.59f,.56f));
        Box(district,"MetroEntranceVoid",new Vector3(8,.7f,111),new Vector3(3,1.4f,6),new Color(.17f,.23f,.24f));
        Box(district,"MetroEntranceCanopy",new Vector3(8,2.7f,111),new Vector3(4,.18f,7),new Color(.36f,.5f,.52f));
        foreach(float z in new[]{108,114})foreach(float x in new[]{6.3f,9.7f})Box(district,"MetroCanopyColumn",new Vector3(x,1.35f,z),new Vector3(.12f,2.7f,.12f),new Color(.43f,.47f,.47f));
        var zone=GameObject.Find("Zone_Hub");zone.transform.position=new Vector3(10,1.5f,147);zone.GetComponent<BoxCollider>().size=new Vector3(236,4,186);
        Move("PhoneStall",new Vector3(-22,.6f,65));Move("BuyPhone",new Vector3(-22,1,63.5f));
        Move("Earn_Post",new Vector3(8,.5f,95));Move("Earn_Post_Visual",new Vector3(8,1.2f,95));
    }
    static void Move(string name,Vector3 at){var go=GameObject.Find(name);if(go!=null)go.transform.position=at;}
    static void Building(Transform p,Lot lot)
    {
        string name=lot.id.StartsWith("H0")?"TownHouse_"+(int.Parse(lot.id.Substring(1))-1):"Parcel_"+lot.id;
        var go=new GameObject(name);go.transform.SetParent(p,false);go.transform.localPosition=new Vector3(lot.x,0,lot.z);
        var info=go.AddComponent<TownParcel>();info.parcelId=lot.id;info.use=lot.use;info.reference="凤起路周边街道照片形态参考；非实测地块";info.footprint=new Vector2(lot.width,lot.depth);info.plannedHeight=lot.height;
        var t=go.transform;
        Color wall=lot.height>25?new Color(.50f,.55f,.55f):lot.height>12?new Color(.73f,.72f,.64f):new Color(.72f,.63f,.48f);
        Box(t,"BuildingBody",new Vector3(0,lot.height/2,0),new Vector3(lot.width,lot.height,lot.depth),wall);
        if(lot.roof==1)
        {
            float angle=18,half=lot.depth*.5f;
            foreach(float side in new[]{-1f,1f})
            {
                var roof=Box(t,"PitchedRoof",new Vector3(0,lot.height+.9f,side*half*.5f),new Vector3(lot.width+.5f,.18f,half/Mathf.Cos(angle*Mathf.Deg2Rad)+.2f),new Color(.35f,.32f,.29f));
                roof.transform.localRotation=Quaternion.Euler(side*angle,0,0);
            }
        }
        else Box(t,"FlatRoof",new Vector3(0,lot.height+.15f,0),new Vector3(lot.width+.4f,.3f,lot.depth+.4f),new Color(.43f,.46f,.45f));
        int floors=Mathf.Max(1,Mathf.FloorToInt(lot.height/3));
        foreach(float side in new[]{-1f,1f})for(int f=0;f<floors;f++)for(float x=-lot.width/2+2;x<lot.width/2-1;x+=3.4f)
        {
            float y=1.7f+f*3;
            Box(t,"WindowGrid",new Vector3(x,y,side*(lot.depth/2+.04f)),new Vector3(1.35f,1.55f,.06f),new Color(.27f,.39f,.4f),false);
            if(f==0 && lot.height<=10)
            {
                Box(t,"Shopfront",new Vector3(x,1.25f,side*(lot.depth/2+.09f)),new Vector3(2.7f,2.5f,.08f),new Color(.21f,.31f,.32f),false);
                Box(t,"ShopAwning",new Vector3(x,2.8f,side*(lot.depth/2+.45f)),new Vector3(2.9f,.12f,1),new Color(.38f,.32f,.24f),false);
            }
        }
        if(lot.height>25)Box(t,"RooftopPlant",new Vector3(lot.width*.15f,lot.height+1.8f,0),new Vector3(lot.width*.45f,3.3f,lot.depth*.6f),wall);
    }
}

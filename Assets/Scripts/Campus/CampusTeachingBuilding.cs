using UnityEngine;

/// <summary>Three accessible storeys, covered outdoor galleries and a playable 201 classroom.</summary>
public static class CampusTeachingBuilding
{
    static readonly Color Ochre=new Color(.87f,.74f,.47f), Red=new Color(.57f,.22f,.15f), Floor=new Color(.66f,.65f,.59f);
    static GameObject Part(Transform p,string n,Vector3 at,Vector3 size,Color c,bool solid=true)
    {return LevelBuildUtil.Make(PrimitiveType.Cube,p,n,at,size,Quaternion.identity,c,"Campus_"+n,solid);}
    static void Remove(Transform p,string n){var t=p.Find(n);if(t!=null)LevelBuildUtil.DestroyObject(t.gameObject);}
    public static void Build(Transform root)
    {
        var building=new GameObject("TeachingBuilding_OpenFloors").transform;building.SetParent(root,false);
        var exam=GameObject.Find("ExamHall").transform;exam.SetParent(building,true);
        var corridor=GameObject.Find("SchoolBuilding").transform;corridor.SetParent(building,true);
        // The old broad corridor slab and east wall intruded into the new ground classroom.
        var ground=corridor.Find("CorridorFloor");ground.localPosition=new Vector3(-15.5f,-.1f,24);ground.localScale=new Vector3(7,.2f,56);
        Remove(corridor,"CorrWallR_A");Remove(corridor,"UpperRightRail");
        Part(corridor,"BalconyReturnRail",new Vector3(-11.8f,4.55f,17.2f),new Vector3(.12f,1.1f,9.6f),Red);
        var left=corridor.Find("UpperLeftRail");left.localPosition=new Vector3(-17,4.55f,9);left.localScale=new Vector3(.16f,1.1f,26);
        // A second actual doorway opens 201 onto the building's front gallery.
        Remove(exam,"WallFront");
        exam.Find("Blackboard").localPosition=new Vector3(-7.5f,2.05f,-3.85f);
        exam.Find("ChalkTray").localPosition=new Vector3(-7.5f,1.39f,-3.76f);
        exam.Find("Podium").localPosition=new Vector3(-5.2f,.51f,-2.6f);
        exam.Find("ExamCeiling").localScale=new Vector3(9.4f,.2f,12);
        Part(exam,"WallFrontLeft",new Vector3(-10.5f,1.7f,8),new Vector3(3.6f,3.4f,.2f),Ochre);
        Part(exam,"WallFrontRight",new Vector3(-4.5f,1.7f,8),new Vector3(3.6f,3.4f,.2f),Ochre);
        Part(exam,"FrontDoorLintel",new Vector3(-7.5f,2.9f,8),new Vector3(2.4f,1,.2f),Ochre);
        var exit=new GameObject("ExamGalleryExit");exit.transform.SetParent(exam,false);exit.transform.localPosition=new Vector3(-7.5f,1,8.4f);
        var trigger=exit.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.size=new Vector3(2.2f,2.2f,1);
        exit.AddComponent<ExamExitTrigger>();
        for(int level=0;level<3;level++)
        {
            float y=level*4;
            var storey=new GameObject("Floor_"+(level+1)).transform;storey.SetParent(building,false);
            for(int row=0;row<2;row++)for(int col=0;col<5;col++)
            {
                if(level==1 && row==0 && col==0)continue;
                float x=col==0?-7.5f:3+(col-1)*12;
                float z=row==0?2:-26;
                int number=(level+1)*100+1+row*5+col;
                Room(storey,new Vector3(x,y,z),col==0?9:10,number,level==0 && row==0 && col==1);
            }
            Gallery(storey,"FrontGallery",y,10.2f);
            Gallery(storey,"CourtyardGallerySouth",y,-6.2f);
            Gallery(storey,"CourtyardGalleryNorth",y,-18);
            // Bridges surround an open air courtyard. Every classroom stays roofed.
            foreach(float x in new[]{-10f,42f})
            {
                Part(storey,"CourtyardBridge",new Vector3(x,y-.1f,-12.2f),new Vector3(3.2f,.2f,7.6f),Floor);
                Part(storey,"BridgeCeiling",new Vector3(x,y+3.5f,-12.2f),new Vector3(3.3f,.2f,7.6f),Ochre);
                if(level>0)foreach(float side in new[]{-1.6f,1.6f})Part(storey,"BridgeRail",new Vector3(x+side,y+.55f,-12.1f),new Vector3(.1f,1.1f,7.7f),Red);
            }
            if(level>0)
                Part(storey,"GallerySideRail",new Vector3(44.35f,y+.55f,10.2f),new Vector3(.1f,1.1f,4.1f),Red);
        }
        // Ground access remains open beneath the covered front gallery.
        Part(building,"CourtyardEntrancePath",new Vector3(16,-.025f,13.6f),new Vector3(56,.15f,2.6f),Floor);
        EastStairs(building);
        // These are small physical signs, depth-tested by CampusWorldSign.
        Label(building,"201 考场",new Vector3(-7.5f,6.75f,8.2f));
        Label(building,"二楼 · 203 维修钥匙",new Vector3(15,6.75f,8.2f));
        CampusWorldBuilder.CreateTeacher(building,"Patrol_Gallery",new Vector3(29,4,10),Quaternion.Euler(0,90,0),
            new[]{new Vector3(25,4,10),new Vector3(39,4,10),new Vector3(39,4,-6)},1.1f,3.2f,ScrutinyAura.Role.Patrol,7,55);
    }
    static void Gallery(Transform p,string name,float y,float z)
    {
        float depth=z==-18?4:4.4f;
        Part(p,name,new Vector3(16.3f,y-.1f,z),new Vector3(56.2f,.2f,depth),Floor);
        Part(p,name+"Ceiling",new Vector3(16.3f,y+3.5f,z),new Vector3(56.2f,.2f,depth),Ochre);
        for(float x=-10;x<45;x+=8)Part(p,"GalleryColumn",new Vector3(x,y+1.7f,z+1.85f),new Vector3(.22f,3.4f,.22f),Ochre);
        if(y>.1f)
        {
            float edge=z==10.2f?12.4f:z==-18?-16:-8.4f;
            if(z==10.2f)Part(p,"GalleryParapet",new Vector3(16.3f,y+.55f,edge),new Vector3(56.2f,1.1f,.14f),Red);
            else Part(p,"CourtyardParapet",new Vector3(16,y+.55f,edge),new Vector3(48.8f,1.1f,.14f),Red);
        }
    }
    static void Room(Transform p,Vector3 at,float width,int number,bool utility)
    {
        var room=new GameObject("Classroom_"+number).transform;room.SetParent(p,false);room.localPosition=at;
        Part(room,"RoomFloor",new Vector3(0,-.1f,0),new Vector3(width,.2f,12),Floor);
        Part(room,"ClassroomCeiling",new Vector3(0,3.5f,0),new Vector3(width,.2f,12),Ochre);
        foreach(float side in new[]{-1f,1f})
        {
            Part(room,"RoomSideWall",new Vector3(side*width*.5f,1.7f,0),new Vector3(.16f,3.4f,12),Ochre);
            // One front door, one rear door: short loops instead of long dead ends.
            if(side<0 && utility){Part(room,"UtilityBackWall",new Vector3(0,1.7f,-6),new Vector3(width,3.4f,.18f),Ochre);continue;}
            float segment=(width-1.8f)*.5f;
            foreach(float leftRight in new[]{-1f,1f})
            {
                Part(room,"DoorSideWall",new Vector3(leftRight*(.9f+segment*.5f),1.7f,side*6),new Vector3(segment,3.4f,.18f),Ochre);
                Part(room,"WindowInset",new Vector3(leftRight*(width*.3f),1.9f,side*6.11f),new Vector3(1.75f,1.3f,.025f),new Color(.26f,.4f,.43f),false);
            }
            Part(room,"DoorHead",new Vector3(0,2.9f,side*6),new Vector3(1.8f,1,.18f),Ochre);
            Part(room,"FacadeRedBand",new Vector3(0,.65f,side*6.16f),new Vector3(width,.25f,.08f),Red,false);
        }
        if(!utility)
        {
            foreach(float x in new[]{-2.6f,2.6f})foreach(float z in new[]{-1.5f,2.6f})
            {
                Part(room,"StudyDesk",new Vector3(x,.73f,z),new Vector3(1.2f,.08f,.6f),new Color(.55f,.39f,.22f));
                Part(room,"DeskSupport",new Vector3(x,.35f,z),new Vector3(.8f,.7f,.4f),new Color(.35f,.36f,.33f));
            }
            Part(room,"RoomCupboard",new Vector3(width/2-.6f,1.05f,-3.6f),new Vector3(.9f,2.1f,1.1f),new Color(.4f,.49f,.45f));
        }
        Label(room,utility?"配电间":number.ToString(),new Vector3(0,2.75f,6.2f));
    }
    static void EastStairs(Transform p)
    {
        var stairs=new GameObject("EastStairCore").transform;stairs.SetParent(p,false);
        for(int flight=0;flight<2;flight++)
        {
            float x=48+flight*4;
            for(int i=0;i<20;i++)
            {
                float top=flight*4+(i+1)*.2f;
                float z=flight==0?9.75f-i*.5f:.25f+i*.5f;
                Part(stairs,"EastStep_"+flight+"_"+i,new Vector3(x,top-.1f,z),new Vector3(2.8f,.2f,.5f),Floor);
            }
            var roof=Part(stairs,"EastStairCeiling",new Vector3(x,flight*4+5.5f,5),new Vector3(3,.2f,10.8f),Ochre);
            roof.transform.localRotation=Quaternion.Euler(flight==0?21.8f:-21.8f,0,0);
            foreach(float side in new[]{-1.5f,1.5f})
            {
                var rail=Part(stairs,"EastStairRail",new Vector3(x+side,flight*4+2.65f,5),new Vector3(.1f,.12f,10.8f),Red);
                rail.transform.localRotation=roof.transform.localRotation;
                for(int i=0;i<=10;i++)
                {float z=i;float top=flight*4+(flight==0?4-i*.4f:i*.4f);
                 Part(stairs,"EastRailPost",new Vector3(x+side,top+.55f,z),new Vector3(.08f,1.1f,.08f),Red);}
            }
        }
        Part(stairs,"EastGroundLanding",new Vector3(48,-.1f,11.4f),new Vector3(3,.2f,2.8f),Floor);
        Part(stairs,"EastMiddleLanding",new Vector3(50,3.9f,-1),new Vector3(7.2f,.2f,2),Floor);
        Part(stairs,"EastAccessWalk",new Vector3(45.4f,3.9f,4.4f),new Vector3(2,.2f,11.2f),Floor);
        Part(stairs,"EastMiddleRoof",new Vector3(48,7.5f,-1),new Vector3(7,.2f,2),Ochre);
        Part(stairs,"EastAccessCeiling",new Vector3(45.4f,7.5f,4.4f),new Vector3(2,.2f,11.2f),Ochre);
        Part(stairs,"EastTopLanding",new Vector3(49.4f,7.9f,11.2f),new Vector3(10,.2f,2.4f),Floor);
        Part(stairs,"EastTopCeiling",new Vector3(49.4f,11.5f,11.2f),new Vector3(9.8f,.2f,2.4f),Ochre);
        Part(stairs,"EastGroundPath",new Vector3(48,-.025f,14),new Vector3(3,.15f,2.4f),Floor);
        // Opening on the east side of the second-floor front gallery.
        var storey=p.Find("Floor_2");Remove(storey,"GallerySideRail");
        Remove(p.Find("Floor_3"),"GallerySideRail");
    }
    static void Label(Transform p,string value,Vector3 at)
    {
        var go=new GameObject("RoomSign");go.transform.SetParent(p,false);go.transform.localPosition=at;go.transform.localRotation=Quaternion.Euler(0,180,0);
        var t=go.AddComponent<TextMesh>();t.text=value;t.anchor=TextAnchor.MiddleCenter;t.characterSize=.07f;t.fontSize=40;t.color=new Color(1,.95f,.75f);
        t.font=Resources.Load<Font>("UIFont");if(t.font!=null)go.GetComponent<Renderer>().sharedMaterial=t.font.material;go.AddComponent<CampusWorldSign>();
    }
}

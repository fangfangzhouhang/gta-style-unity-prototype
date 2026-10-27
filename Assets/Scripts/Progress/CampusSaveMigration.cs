using System.Collections.Generic;

/// <summary>Version 2: preserve legacy economy and completed campus runs; restart unsupported pre-chapter partial locks safely.</summary>
public static class CampusSaveMigration
{
    public static void Upgrade(SaveData d)
    {
        if(d==null)return;
        bool legacy=d.version<2;
        bool noChapter=d.examChapter==null;
        d.examChapter=d.examChapter??new CampusExamState();
        d.corridorChapter=d.corridorChapter??new CampusCorridorState();
        d.libraryChapter=d.libraryChapter??new CampusLibraryState();
        d.departureChapter=d.departureChapter??new CampusDepartureState();
        d.journal=d.journal??new CampusJournalState();d.narrative=d.narrative??new CampusNarrativeState();
        d.journal.visited=d.journal.visited??new List<string>();d.narrative.played=d.narrative.played??new List<string>();
        if(legacy && d.sceneName=="CampusWorld")
        {
            if(d.escapedSchool)
            {
                // A previously completed v11 route must not be trapped behind new chapter locks.
                d.examChapter=new CampusExamState{paper=true,chart=true,key=true,sideOpen=true,introduction=true,corridorIntroduction=true};
                d.corridorChapter=new CampusCorridorState{notice=true,conversation=true,card=true,roomOpen=true,record=true,lighting=true,alarm=false,doorPower=true,circuit=true};
                d.libraryChapter=new CampusLibraryState{indexRead=true,recordRead=true,manual=true,key=true,downOpen=true,utilityOpen=true,emergency=true};
                d.departureChapter=new CampusDepartureState{gateOpen=true,departed=true,completed=true};
                d.mapUnlocked=true;d.stage=GameProgress.StageHub;d.checkpointId="HubCheckpoint";d.hasPlayerPose=false;
            }
            else if(noChapter)
            {
                d.stage=GameProgress.StageExam;d.mapUnlocked=false;d.hasPlayerPose=false;d.hasCheckpoint=false;d.checkpointId="ExamCheckpoint";
            }
            var e=d.examChapter;var c=d.corridorChapter;var l=d.libraryChapter;var f=d.departureChapter;
            Seen(d,"intro",e.introduction);Seen(d,"window",e.window);Seen(d,"key",e.key);Seen(d,"side",e.sideOpen);Seen(d,"corridor",e.corridorIntroduction);
            Seen(d,"room203",c.roomOpen);Seen(d,"circuit",c.circuit);Seen(d,"archive",l.manual);Seen(d,"emergency",l.emergency);
            Seen(d,"gate",f.gateOpen);Seen(d,"departure",f.departed);Seen(d,"complete",f.completed);
            Visit(d,"201",true);Visit(d,"corridor",e.corridorIntroduction);Visit(d,"203",c.record);Visit(d,"library",l.indexRead||l.recordRead);Visit(d,"service",l.emergency);Visit(d,"outside",f.departed);Visit(d,"town",f.completed);
        }
        d.version=2;
    }
    static void Seen(SaveData d,string id,bool fact){if(fact && !d.narrative.played.Contains(id))d.narrative.played.Add(id);}
    static void Visit(SaveData d,string id,bool fact){if(fact && !d.journal.visited.Contains(id))d.journal.visited.Add(id);}
}

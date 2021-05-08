using System.Collections.Generic;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Model {
    public class TsMap {
        // public List< TsRoadSideItem >      RoadSideItems   = new List< TsRoadSideItem >();
        // public List< TsServiceItem >       ServiceItems    = new List< TsServiceItem >();
        // public List< TsTrafficRuleItem >   RuleItems       = new List< TsTrafficRuleItem >();
        // public List< TsTrajectoryItem >    TrajectoryItems = new List< TsTrajectoryItem >();
        // public List< TsTriggerItem >       TriggerItems    = new List< TsTriggerItem >();
        public Dictionary< ulong, TsNode > Nodes = new Dictionary< ulong, TsNode >();

        // public List< TsBusStopItem >       BusStopItems    = new List< TsBusStopItem >();
        // public List< TsCutPlaneItem >      CutPlaneItems   = new List< TsCutPlaneItem >();
        // public List< TsCutsceneItem >      CutsceneItems   = new List< TsCutsceneItem >();
        // public List< TsFerryItem >         FerryItems      = new List< TsFerryItem >();
        // public List< TsFuelPumpItem >      FuelPumpItems   = new List< TsFuelPumpItem >();
        // public List< TsGarageItem >        GarageItems     = new List< TsGarageItem >();
        // public List< TsMapAreaItem >       MapAreaItems    = new List< TsMapAreaItem >();
        // public List< TsMapOverlayItem >    MapOverlayItems = new List< TsMapOverlayItem >();
        // public List< TsPrefabItem >        PrefabItems     = new List< TsPrefabItem >();
        public List< TsRoadItem > RoadItems = new List< TsRoadItem >();

        public TsNode GetNodeByUid( ulong uid ) =>
            this.Nodes.ContainsKey( uid )
                ? this.Nodes[ uid ]
                : null;
    }
}
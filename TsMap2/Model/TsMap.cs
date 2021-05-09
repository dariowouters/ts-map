using System.Collections.Generic;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Model {
    public class TsMap {
        public readonly List< TsCityItem >          Cities           = new List< TsCityItem >();
        public readonly List< TsCompanyItem >       Companies        = new List< TsCompanyItem >();
        public readonly List< TsFerryItem >         FerryConnections = new List< TsFerryItem >();
        public readonly List< TsMapAreaItem >       MapAreas         = new List< TsMapAreaItem >();
        public readonly List< TsMapOverlayItem >    MapOverlays      = new List< TsMapOverlayItem >();
        public readonly List< TsPrefabItem >        Prefabs          = new List< TsPrefabItem >();
        public readonly List< TsRoadItem >          Roads            = new List< TsRoadItem >();
        public readonly List< TsTriggerItem >       Triggers         = new List< TsTriggerItem >();
        public          Dictionary< ulong, TsNode > Nodes            = new Dictionary< ulong, TsNode >();

        public TsNode GetNodeByUid( ulong uid ) =>
            this.Nodes.ContainsKey( uid )
                ? this.Nodes[ uid ]
                : null;
    }
}
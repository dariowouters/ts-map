using System.Collections.Generic;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Model {
    public class TsMap {
        public readonly List< TsMapCityItem >       Cities           = new List< TsMapCityItem >();
        public readonly List< TsMapCompanyItem >    Companies        = new List< TsMapCompanyItem >();
        public readonly List< TsMapFerryItem >      FerryConnections = new List< TsMapFerryItem >();
        public readonly List< TsMapAreaItem >       MapAreas         = new List< TsMapAreaItem >();
        public readonly List< TsMapMapOverlayItem > MapOverlays      = new List< TsMapMapOverlayItem >();
        public readonly List< TsMapPrefabItem >     Prefabs          = new List< TsMapPrefabItem >();
        public readonly List< TsMapRoadItem >       Roads            = new List< TsMapRoadItem >();
        public readonly List< TsMapTriggerItem >    Triggers         = new List< TsMapTriggerItem >();
        public          float                       MaxX             = float.MinValue;
        public          float                       MaxZ             = float.MinValue;
        public          float                       MinX             = float.MaxValue;
        public          float                       MinZ             = float.MaxValue;
        public          Dictionary< ulong, TsNode > Nodes            = new Dictionary< ulong, TsNode >();
        public          TsMapOverlays               Overlays         = new TsMapOverlays();

        public TsNode GetNodeByUid( ulong uid ) =>
            Nodes.ContainsKey( uid )
                ? Nodes[ uid ]
                : null;

        public void AddNode( TsNode node ) {
            if ( !Nodes.ContainsKey( node.Uid ) )
                Nodes.Add( node.Uid, node );
        }

        public void UpdateEdgeCoords( TsNode node ) {
            if ( MinX > node.X ) MinX = node.X;
            if ( MaxX < node.X ) MaxX = node.X;
            if ( MinZ > node.Z ) MinZ = node.Z;
            if ( MaxZ < node.Z ) MaxZ = node.Z;
        }
    }
}
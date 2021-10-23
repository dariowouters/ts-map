using System.Collections.Generic;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Model {
    public class TsMap {
        public readonly Dictionary< ulong, TsMapCityItem > Cities           = new();
        public readonly List< TsMapCompanyItem >           Companies        = new();
        public readonly List< TsMapFerryItem >             FerryConnections = new();
        public readonly List< TsMapAreaItem >              MapAreas         = new();
        public readonly List< TsMapMapOverlayItem >        MapOverlays      = new();
        public readonly List< TsMapPrefabItem >            Prefabs          = new();
        public          List< TsMapRoadItem >              Roads            = new();
        public readonly List< TsMapTriggerItem >           Triggers         = new();
        public          float                              MaxX             = float.MinValue;
        public          float                              MaxZ             = float.MinValue;
        public          float                              MinX             = float.MaxValue;
        public          float                              MinZ             = float.MaxValue;
        public          Dictionary< ulong, TsNode >        Nodes            = new();
        public          TsMapOverlays                      Overlays         = new();

        public TsNode? GetNodeByUid( ulong uid ) =>
            Nodes.ContainsKey( uid )
                ? Nodes[ uid ]
                : null;

        public void AddNode( TsNode node ) {
            if ( !Nodes.ContainsKey( node.Uid ) )
                Nodes.Add( node.Uid, node );
        }

        public void AddItem( TsMapItem.TsMapItem mapItem ) {
            if ( !mapItem.Valid ) return;

            switch ( mapItem ) {
                case TsMapRoadItem item:
                    if ( !item.Hidden )
                        Roads.Add( item );
                    break;
                case TsMapPrefabItem item:
                    Prefabs.Add( item );
                    break;
                case TsMapCompanyItem item:
                    Companies.Add( item );
                    break;
                case TsMapCityItem item:
                    if ( !Cities.ContainsKey( item.City.Token ) && !item.Hidden )
                        Cities.Add( item.City.Token, item );

                    break;
                case TsMapMapOverlayItem item:
                    MapOverlays.Add( item );
                    break;
                case TsMapFerryItem item:
                    FerryConnections.Add( item );
                    break;
                case TsMapTriggerItem item:
                    Triggers.Add( item );
                    break;
                case TsMapAreaItem item:
                    MapAreas.Add( item );
                    break;
            }
        }

        public void UpdateEdgeCoords( TsNode node ) {
            if ( MinX > node.X ) MinX = node.X;
            if ( MaxX < node.X ) MaxX = node.X;
            if ( MinZ > node.Z ) MinZ = node.Z;
            if ( MaxZ < node.Z ) MaxZ = node.Z;
        }
    }
}
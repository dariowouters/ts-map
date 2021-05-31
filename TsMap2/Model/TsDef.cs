using System.Collections.Generic;
using System.Linq;
using TsMap2.Helper;

namespace TsMap2.Model {
    public class TsDef {
        public Dictionary< ulong, TsCity >       Cities           = new Dictionary< ulong, TsCity >();
        public Dictionary< ulong, TsCountry >    Countries        = new Dictionary< ulong, TsCountry >();
        public List< TsFerryConnection >         FerryConnections = new List< TsFerryConnection >();
        public Dictionary< ulong, TsMapOverlay > Overlays         = new Dictionary< ulong, TsMapOverlay >();
        public Dictionary< ulong, TsPrefab >     Prefabs          = new Dictionary< ulong, TsPrefab >();
        public Dictionary< ulong, TsRoadLook >   RoadLooks        = new Dictionary< ulong, TsRoadLook >();

        public TsCountry GetCountryByTokenName( string name ) {
            ulong token = ScsHashHelper.StringToToken( name );
            return Countries.ContainsKey( token )
                       ? Countries[ token ]
                       : null;
        }


        // public void AddRoadLook( TsRoadLook roadLook ) {
        //     if ( roadLook.Token != 0 && !RoadLooks.ContainsKey( roadLook.Token ) ) // Log.Debug( "R: {0}", roadLook.Token );
        //         RoadLooks.Add( roadLook.Token, roadLook );
        // }

        public TsRoadLook LookupRoadLook( ulong lookId ) =>
            // Log.Debug( "L: {0}", lookId );
            RoadLooks.ContainsKey( lookId )
                ? RoadLooks[ lookId ]
                : null;

        public TsPrefab LookupPrefab( ulong prefabId ) =>
            Prefabs.ContainsKey( prefabId )
                ? Prefabs[ prefabId ]
                : null;

        public TsCity LookupCity( ulong cityId ) =>
            Cities.ContainsKey( cityId )
                ? Cities[ cityId ]
                : null;

        public TsMapOverlay LookupOverlay( ulong overlayId ) =>
            Overlays.ContainsKey( overlayId )
                ? Overlays[ overlayId ]
                : null;

        public List< TsFerryConnection > LookupFerryConnection( ulong ferryPortId ) {
            return FerryConnections.Where( item => item.StartPortToken == ferryPortId ).ToList();
        }

        // public void AddCountry( TsCountry tsCountry ) {
        //     if ( tsCountry.Token != 0 && !Countries.ContainsKey( tsCountry.Token ) )
        //         Countries.Add( tsCountry.Token, tsCountry );
        // }

        // public void AddPrefab( TsPrefab prefab ) {
        //     if ( prefab.Token != 0 && !Prefabs.ContainsKey( prefab.Token ) )
        //         Prefabs.Add( prefab.Token, prefab );
        // }

        // public void AddFerryConnection( TsFerryConnection ferryConnection ) {
        //     TsFerryConnection existingItem = FerryConnections.FirstOrDefault( item =>
        //                                                                                item.StartPortToken  == ferryConnection.StartPortToken
        //                                                                                && item.EndPortToken == ferryConnection.EndPortToken
        //                                                                                || item.StartPortToken == ferryConnection.EndPortToken
        //                                                                                && item.EndPortToken
        //                                                                                == ferryConnection
        //                                                                                    .StartPortToken ); // Check if ferryConnectionection already exists
        //     if ( existingItem == null ) FerryConnections.Add( ferryConnection );
        // }

        // public void AddCity( TsCity city ) {
        //     if ( city.Token != 0 && !Cities.ContainsKey( city.Token ) )
        //         Cities.Add( city.Token, city );
        // }

        // public void AddOverlay( TsMapOverlay mapOverlay ) {
        //     if ( !Overlays.ContainsKey( mapOverlay.Token ) )
        //         Overlays.Add( mapOverlay.Token, mapOverlay );
        // }

        public void AddFerryPortLocation( ulong ferryPortId, float x, float z ) {
            IEnumerable< TsFerryConnection > ferry =
                FerryConnections.Where( item => item.StartPortToken  == ferryPortId
                                                || item.EndPortToken == ferryPortId );
            foreach ( TsFerryConnection connection in ferry ) connection.SetPortLocation( ferryPortId, x, z );
        }
    }
}
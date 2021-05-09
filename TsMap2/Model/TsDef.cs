using System.Collections.Generic;
using System.Linq;
using TsMap2.Scs;

namespace TsMap2.Model {
    public class TsDef {
        public Dictionary< ulong, TsCity >       Cities           = new Dictionary< ulong, TsCity >();
        public Dictionary< ulong, TsCountry >    Countries        = new Dictionary< ulong, TsCountry >();
        public List< TsFerryConnection >         FerryConnections = new List< TsFerryConnection >();
        public Dictionary< ulong, TsMapOverlay > Overlays         = new Dictionary< ulong, TsMapOverlay >();
        public Dictionary< ulong, TsPrefab >     Prefabs          = new Dictionary< ulong, TsPrefab >();
        public Dictionary< ulong, TsRoadLook >   RoadLooks        = new Dictionary< ulong, TsRoadLook >();

        public TsCountry GetCountryByTokenName( string name ) {
            ulong token = ScsHash.StringToToken( name );
            return this.Countries.ContainsKey( token )
                       ? this.Countries[ token ]
                       : null;
        }


        public void AddRoadLook( TsRoadLook roadLook ) {
            if ( roadLook.Token != 0 && !this.RoadLooks.ContainsKey( roadLook.Token ) ) // Log.Debug( "R: {0}", roadLook.Token );
                this.RoadLooks.Add( roadLook.Token, roadLook );
        }

        public TsRoadLook LookupRoadLook( ulong lookId ) =>
            // Log.Debug( "L: {0}", lookId );
            this.RoadLooks.ContainsKey( lookId )
                ? this.RoadLooks[ lookId ]
                : null;

        public TsPrefab LookupPrefab( ulong prefabId ) =>
            this.Prefabs.ContainsKey( prefabId )
                ? this.Prefabs[ prefabId ]
                : null;

        public TsCity LookupCity( ulong cityId ) =>
            this.Cities.ContainsKey( cityId )
                ? this.Cities[ cityId ]
                : null;

        public TsMapOverlay LookupOverlay( ulong overlayId ) =>
            this.Overlays.ContainsKey( overlayId )
                ? this.Overlays[ overlayId ]
                : null;

        public List< TsFerryConnection > LookupFerryConnection( ulong ferryPortId ) {
            return this.FerryConnections.Where( item => item.StartPortToken == ferryPortId ).ToList();
        }

        public void AddCountry( TsCountry tsCountry ) {
            if ( tsCountry.Token != 0 && !this.Countries.ContainsKey( tsCountry.Token ) )
                this.Countries.Add( tsCountry.Token, tsCountry );
        }

        public void AddPrefab( TsPrefab prefab ) {
            if ( prefab.Token != 0 && !this.Prefabs.ContainsKey( prefab.Token ) )
                this.Prefabs.Add( prefab.Token, prefab );
        }

        public void AddFerryConnection( TsFerryConnection ferryConnection ) {
            TsFerryConnection existingItem = this.FerryConnections.FirstOrDefault( item =>
                                                                                       item.StartPortToken  == ferryConnection.StartPortToken
                                                                                       && item.EndPortToken == ferryConnection.EndPortToken
                                                                                       || item.StartPortToken == ferryConnection.EndPortToken
                                                                                       && item.EndPortToken
                                                                                       == ferryConnection
                                                                                           .StartPortToken ); // Check if ferryConnectionection already exists
            if ( existingItem == null ) this.FerryConnections.Add( ferryConnection );
        }

        public void AddCity( TsCity city ) {
            if ( city.Token != 0 && !this.Cities.ContainsKey( city.Token ) )
                this.Cities.Add( city.Token, city );
        }

        public void AddOverlay( TsMapOverlay mapOverlay ) {
            if ( !this.Overlays.ContainsKey( mapOverlay.Token ) )
                this.Overlays.Add( mapOverlay.Token, mapOverlay );
        }

        public void AddFerryPortLocation( ulong ferryPortId, float x, float z ) {
            IEnumerable< TsFerryConnection > ferry =
                this.FerryConnections.Where( item => item.StartPortToken  == ferryPortId
                                                     || item.EndPortToken == ferryPortId );
            foreach ( TsFerryConnection connection in ferry ) connection.SetPortLocation( ferryPortId, x, z );
        }
    }
}
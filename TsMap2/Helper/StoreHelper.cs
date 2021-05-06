using System.Collections.Generic;
using System.Linq;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Helper {
    public sealed class StoreHelper {
        public Dictionary< ulong, TsCity >     Cities           = new Dictionary< ulong, TsCity >();
        public Dictionary< ulong, TsCountry >  Countries        = new Dictionary< ulong, TsCountry >();
        public List< TsFerryConnection >       FerryConnections = new List< TsFerryConnection >();
        public TsGame                          Game;
        public Dictionary< ulong, TsPrefab >   Prefabs   = new Dictionary< ulong, TsPrefab >();
        public Dictionary< ulong, TsRoadLook > RoadLooks = new Dictionary< ulong, TsRoadLook >();

        // --


        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static StoreHelper() { }

        private StoreHelper() { }
        public Settings       Settings { get; private set; } = new Settings();
        public RootFileSystem Rfs      { get; private set; }

        public static StoreHelper Instance { get; } = new StoreHelper();

        // ---

        public void SetSetting( Settings settings ) {
            this.Settings = settings;
            this.Rfs      = new RootFileSystem( settings.GetActiveGamePath() );
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

        public void AddRoadLook( TsRoadLook roadLook ) {
            if ( roadLook.Token != 0 && !this.RoadLooks.ContainsKey( roadLook.Token ) )
                this.RoadLooks.Add( roadLook.Token, roadLook );
        }
    }
}
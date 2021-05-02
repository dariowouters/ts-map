using System.Collections.Generic;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Helper {
    public sealed class StoreHelper {
        public Dictionary< ulong, TsCountry > Countries = new Dictionary< ulong, TsCountry >();
        public TsGame                         Game      = new TsGame();
        public RootFileSystem                 Rfs;

        // --

        public Settings Settings = new Settings();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static StoreHelper() { }

        private StoreHelper() { }

        public static StoreHelper Instance { get; } = new StoreHelper();

        // ---

        public void SetSetting( Settings settings ) {
            this.Settings = settings;
            this.Rfs      = new RootFileSystem( settings.Ets2Path );
        }

        public void AddCountry( TsCountry tsCountry ) {
            if ( tsCountry.Token != 0 && !this.Countries.ContainsKey( tsCountry.Token ) )
                this.Countries.Add( tsCountry.Token, tsCountry );
        }
    }
}
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Helper {
    public sealed class StoreHelper {
        public TsDef       Def = new TsDef();
        public TsGame      Game;
        public Model.TsMap Map = new Model.TsMap();

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
    }
}
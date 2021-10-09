using TsMap2.Model;
using TsMap2.Scs.FileSystem;

namespace TsMap2.Helper {
    public sealed class StoreHelper {
        public readonly TsDef  Def = new();
        public          TsGame Game;
        public readonly TsMap  Map = new();

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
            Settings = settings;
            Rfs      = new RootFileSystem( settings.GetActiveGamePath() );
        }
    }
}
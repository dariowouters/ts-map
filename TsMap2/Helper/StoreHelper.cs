using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2.Helper {
    public sealed class StoreHelper {
        public TsGame         Game = new TsGame();
        public RootFileSystem Rfs;

        // --

        public Settings Settings = new Settings();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static StoreHelper() { }

        private StoreHelper() { }

        public static StoreHelper Instance { get; } = new StoreHelper();

        public void SetSetting( Settings settings ) {
            this.Settings = settings;
            this.Rfs      = new RootFileSystem( settings.GamePath );
        }
    }
}
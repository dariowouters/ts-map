using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefPrefabsJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Prefab] Loading" );

            var prefabEntry = new ScsPrefabEntry();
            Store().Def.Prefabs = prefabEntry.List();

            Log.Information( "[Job][Prefab] Loaded. Found: {0}", Store().Def.Prefabs.Count );
        }
    }
}
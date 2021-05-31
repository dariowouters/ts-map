using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefRoadLooksJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][RoadLook] Loading" );

            var roadLookEntry = new ScsRoadLooksEntry();
            Store().Def.RoadLooks = roadLookEntry.List();

            Log.Information( "[Job][RoadLook] Loaded. Found: {0}", Store().Def.RoadLooks.Count );
        }
    }
}
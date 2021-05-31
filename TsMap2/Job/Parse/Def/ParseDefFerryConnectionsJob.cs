using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefFerryConnectionsJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][FerryConnections] Loading" );

            var ferryConnectionEntry = new ScsFerryConnectionEntry();
            Store().Def.FerryConnections = ferryConnectionEntry.List();

            Log.Information( "[Job][FerryConnections] Loaded. Found: {0}", Store().Def.FerryConnections.Count );
        }
    }
}
using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefCitiesJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][City] Loading" );

            var scsCityEntry = new ScsCityEntry();
            Store().Def.Cities = scsCityEntry.List();

            Log.Information( "[Job][City] Loaded. Found: {0}", Store().Def.Cities.Count );
        }
    }
}
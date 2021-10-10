using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefCountriesJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Country] Loading" );

            var countryEntry = new ScsCountryEntry();
            Store().Def.Countries = countryEntry.List();

            Log.Information( "[Job][Country] Loaded. Found: {0}", Store().Def.Countries.Count );
        }
    }
}
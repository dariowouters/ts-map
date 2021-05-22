using System.Collections.Generic;
using System.Linq;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Model;

namespace TsMap2.Job.Export {
    public class ExportCountriesJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][ExportCountries] Exporting..." );

            List< TsCountry > countries = this.Store().Def.Countries.Values.ToList();

            Log.Debug( "[Job][ExportCountries] To export: {0}", countries.Count );
            var countriesFactory = new TsCountriesJsonFactory( countries );
            countriesFactory.Save();
            Log.Information( "[Job][ExportCountries] Saved !" );
        }
    }
}
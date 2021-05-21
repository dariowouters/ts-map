using System.Collections.Generic;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Model;

namespace TsMap2.Job.Export {
    public class ExportCountriesJob : ThreadJob {
        protected override void Do() {
            // if ( !Directory.Exists( path ) ) return;

            Log.Information( "[Job][ExportCountries] Exporting..." );

            var countries = new List< TsCountry >();
            foreach ( TsCountry country in this.Store().Def.Countries.Values ) // JObject countryJObj = JObject.FromObject( country );
                // if ( exportFlags.IsActive( ExportFlags.CityLocalizedNames ) )
                // countryJObj[ "LocalizedNames" ] = JObject.FromObject( country.LocalizedNames );
                countries.Add( country );

            // FIXME: Check why countries doesn't have localized names
            Log.Debug( "[Job][ExportCountries] To export: {0}", countries.Count );
            var countriesFactory = new TsCountriesJsonFactory( countries );
            countriesFactory.Save();
            Log.Information( "[Job][ExportCountries] Saved !" );
        }

        protected override void OnEnd() { }
    }
}
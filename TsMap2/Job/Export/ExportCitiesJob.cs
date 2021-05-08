using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;

namespace TsMap2.Job.Export {
    public class ExportCitiesJob : ThreadJob {
        protected override void Do() {
            if ( !Directory.Exists( this.Store().Settings.OutputPath ) ) return;
            var citiesJArr = new JArray();

            // TODO: Continue here: Use one only array
            // foreach ( TsCityItem city in this.Cities ) {
            //     if ( city.Hidden ) continue;
            //     JObject cityJObj = JObject.FromObject( city.City );
            //     cityJObj[ "X" ] = city.X;
            //     cityJObj[ "Y" ] = city.Z;
            //     if ( this._countriesLookup.ContainsKey( ScsHash.StringToToken( city.City.Country ) ) ) {
            //         TsCountry country = this._countriesLookup[ ScsHash.StringToToken( city.City.Country ) ];
            //         cityJObj[ "CountryId" ] = country.CountryId;
            //     } else
            //         Log.Warning( $"Could not find country for {city.City.Name}" );
            //
            //     if ( exportFlags.IsActive( ExportFlags.CityLocalizedNames ) )
            //         cityJObj[ "LocalizedNames" ] = JObject.FromObject( city.City.LocalizedNames );
            //
            //     citiesJArr.Add( cityJObj );
            // }

            File.WriteAllText( Path.Combine( this.Store().Settings.OutputPath, AppPath.CitiesFileName ), citiesJArr.ToString( Formatting.Indented ) );
        }

        protected override void OnEnd() { }
    }
}
using System.Collections.Generic;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Export {
    public class ExportCitiesJob : ThreadJob {
        protected override void Do() {
            // if ( !Directory.Exists( this.Store().Settings.OutputPath ) ) return;

            Log.Information( "[Job][ExportCities] Exporting..." );

            var cities = new List< TsCity >();

            // TODO: Match country code between city and country
            foreach ( KeyValuePair< ulong, TsCity > kv in Store().Def.Cities ) {
                TsCity city = kv.Value;

                // if ( city.Hidden ) continue;

                TsMapCityItem item = Store().Map.Cities.Find( c => c.City.Token == city.Token );

                if ( item != null ) {
                    city.X = item.X;
                    city.Y = item.Z;
                }

                if ( Store().Def.Countries.ContainsKey( ScsHashHelper.StringToToken( city.CountryName ) ) ) {
                    TsCountry country = Store().Def.Countries[ ScsHashHelper.StringToToken( city.CountryName ) ];
                    city.CountryId = country.CountryId;
                    // cityJObj[ "CountryId" ] = country.CountryId;
                } else
                    Log.Warning( $"[Job][ExportCities] Could not find country for {city.Name}" );

                // if ( exportFlags.IsActive( ExportFlags.CityLocalizedNames ) )
                // cityJObj[ "LocalizedNames" ] = JObject.FromObject( city.City.LocalizedNames );

                // citiesJArr.Add( cityJObj );
                cities.Add( city );
            }

            Log.Debug( "[Job][ExportCities] To export: {0}", cities.Count );
            var cityFactory = new TsCitiesJsonFactory( cities );
            cityFactory.Save();
            Log.Information( "[Job][ExportCities] Saved !" );
        }
    }
}
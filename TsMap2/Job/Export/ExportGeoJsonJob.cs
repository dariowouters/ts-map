using System.Collections.Generic;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Model.GeoJson;

namespace TsMap2.Job.Export {
    public class ExportGeoJsonJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][ExportGeoJson] Exporting..." );

            var geoJson = new GeoJson();

            foreach ( KeyValuePair< ulong, TsCity > kv in Store().Def.Cities ) {
                TsCity city = kv.Value;

                geoJson.Features.Add( new Feature( city ) );
            }

            var geoJsonFactory = new GeoJsonFactory( AppPath.GeoJsonCities, geoJson );
            geoJsonFactory.Save();

            Log.Information( "[Job][ExportGeoJson] Saved !" );
        }
    }
}
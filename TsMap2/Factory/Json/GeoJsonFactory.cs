using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TsMap2.Model.GeoJson;

namespace TsMap2.Factory.Json {
    public class GeoJsonFactory : JsonFactory< GeoJson > {
        public GeoJsonFactory( string fileName, GeoJson geoJson ) {
            _fileName = fileName;
            _geoJson  = geoJson;
        }

        private string _fileName { get; }

        private GeoJson _geoJson { get; }

        public override string GetFileName() => _fileName;

        public override string GetSavingPath() => Path.Combine( Store.Settings.OutputPath, Store.Game.Code, "latest/" );

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override GeoJson Convert( JObject raw ) => raw.ToObject< GeoJson >();

        public override JContainer RawData() => JObject.FromObject( _geoJson );
    }
}
using Newtonsoft.Json.Linq;
using TsMap2.Model;

namespace TsMap2.Factory.Json {
    public class TsSettingsJsonFactory : JsonFactory< Settings > {
        public TsSettingsJsonFactory( Settings settings ) => this._settings = settings;

        private Settings _settings { get; }

        public override string GetFileName() => "Settings.json";

        public override string GetSavingPath() => "Output/";

        public override string GetLoadingPath() => "Output/";

        public override Settings Convert( JObject raw ) => raw.ToObject< Settings >();

        public override JObject RawData() => JObject.FromObject( this._settings );
    }
}
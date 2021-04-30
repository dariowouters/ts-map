using Newtonsoft.Json.Linq;
using TsMap2.Model;

namespace TsMap2.Factory.Json {
    public class TsSettingsJsonFactory< T > : JsonFactory< T > {
        public TsSettingsJsonFactory( Settings settings ) => this._settings = settings;

        private Settings _settings { get; }

        public override string GetFileName() => "Settings.json";

        public override string GetSavingPath() => "Output/";

        public override T Convert( JObject raw ) => raw.ToObject< T >();

        public override JObject RawData() =>
            new JObject {
                [ "name" ]     = this._settings.Name,
                [ "GamePath" ] = this._settings.GamePath
            };
    }
}
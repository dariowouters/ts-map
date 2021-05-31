using Newtonsoft.Json.Linq;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Factory.Json {
    public class SettingsJsonFactory : JsonFactory< Settings > {
        public SettingsJsonFactory( Settings settings ) => _settings = settings;

        private Settings _settings { get; }

        public override string GetFileName() => AppPath.SettingFileName;

        public override string GetSavingPath() => AppPath.HomeDirApp;

        public override string GetLoadingPath() => AppPath.HomeDirApp;

        public override Settings Convert( JObject raw ) => raw.ToObject< Settings >();

        public override JContainer RawData() => JObject.FromObject( _settings );
    }
}
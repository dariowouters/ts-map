using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory {
    public class TsSettingsFile : IFactory {
        public string GetFileName() => "Settings.json";

        public string GetSavingPath() => throw new NotImplementedException();

        public JObject JsonData() => throw new NotImplementedException();
    }
}
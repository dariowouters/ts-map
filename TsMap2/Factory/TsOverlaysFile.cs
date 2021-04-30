using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory {
    public class TsOverlaysFile : IFactory {
        public string GetFileName() => "Overlays.json";

        public string GetSavingPath() => throw new NotImplementedException();

        public JObject JsonData() => throw new NotImplementedException();
    }
}
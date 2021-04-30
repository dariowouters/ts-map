using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory {
    public class TsCitiesFile : IFactory {
        public string GetFileName() => "Cities.json";

        public string GetSavingPath() => throw new NotImplementedException();

        public JObject JsonData() => throw new NotImplementedException();
    }
}
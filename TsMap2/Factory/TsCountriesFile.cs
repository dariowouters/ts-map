using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory {
    public class TsCountriesFile : IFactory {
        public string GetFileName() => "Countries.json";

        public string GetSavingPath() => throw new NotImplementedException();

        public JObject JsonData() => throw new NotImplementedException();
    }
}
using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json {
    public class TsCountriesJsonFactory : Factory {
        public override string GetFileName() => "Countries.json";

        public override string GetSavingPath() => throw new NotImplementedException();

        public override JObject JsonData() => throw new NotImplementedException();
    }
}
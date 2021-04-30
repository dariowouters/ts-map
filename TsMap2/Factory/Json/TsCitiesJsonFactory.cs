using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json {
    public class TsCitiesJsonFactory : Factory {
        public override string GetFileName() => "Cities.json";

        public override string GetSavingPath() => throw new NotImplementedException();

        public override JObject JsonData() => throw new NotImplementedException();
    }
}
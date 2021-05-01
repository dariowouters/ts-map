using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json {
    public class TsCountriesJsonFactory< T > : JsonFactory< T > {
        public override string GetFileName() => "Countries.json";

        public override string GetSavingPath()  => throw new NotImplementedException();
        public override string GetLoadingPath() => throw new NotImplementedException();

        public override T Convert( JObject raw ) => throw new NotImplementedException();

        public override JObject RawData() => throw new NotImplementedException();
    }
}
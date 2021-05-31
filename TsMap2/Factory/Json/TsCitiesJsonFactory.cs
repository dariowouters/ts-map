using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Factory.Json {
    public class TsCitiesJsonFactory : JsonFactory< List< TsCity > > {
        public TsCitiesJsonFactory( List< TsCity > cities ) => _cities = cities;
        private List< TsCity > _cities { get; }

        public override string GetFileName() => AppPath.CitiesFileName;

        public override string GetSavingPath() => Store.Settings.OutputPath;

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override List< TsCity > Convert( JObject raw ) => raw.ToObject< List< TsCity > >();

        public override JContainer RawData() => JArray.FromObject( _cities );
    }
}
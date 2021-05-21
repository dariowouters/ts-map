using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Factory.Json {
    public class TsCountriesJsonFactory : JsonFactory< List< TsCountry > > {
        public TsCountriesJsonFactory( List< TsCountry > countries ) => this._countries = countries;
        private List< TsCountry > _countries { get; }

        public override string GetFileName() => AppPath.CountriesFileName;

        public override string GetSavingPath() => this.Store.Settings.OutputPath;

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override List< TsCountry > Convert( JObject raw ) => raw.ToObject< List< TsCountry > >();

        public override JContainer RawData() => JArray.FromObject( this._countries );
    }
}
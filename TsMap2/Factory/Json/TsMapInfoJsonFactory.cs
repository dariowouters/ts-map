using System;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;
using TsMap2.Model.TsMapInfo;

namespace TsMap2.Factory.Json {
    public class TsMapInfoJsonFactory : JsonFactory< JObject > {
        private readonly TsMapInfo _mapInfo;

        public TsMapInfoJsonFactory( TsMapInfo mapInfo ) => this._mapInfo = mapInfo;

        public override string GetFileName() => AppPath.TileMapInfoFileName;

        public override string GetSavingPath() => this.Store.Settings.OutputPath;

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override JObject Convert( JObject raw ) => throw new NotImplementedException();

        public override JContainer RawData() => this._mapInfo.TileMapInfo();
    }
}
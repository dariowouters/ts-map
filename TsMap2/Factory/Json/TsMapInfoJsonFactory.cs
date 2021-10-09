using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;
using TsMap2.Model.TsMapInfo;

namespace TsMap2.Factory.Json {
    public class TsMapInfoJsonFactory : JsonFactory< JObject > {
        private readonly TsMapInfo _mapInfo;

        public TsMapInfoJsonFactory( TsMapInfo mapInfo ) => _mapInfo = mapInfo;

        public override string GetFileName() => AppPath.TileMapInfoFileName;

        public override string GetSavingPath() => Path.Combine( Store.Settings.OutputPath, Store.Game.Code, "latest/" );

        public override string GetLoadingPath() => throw new NotImplementedException();

        public override JObject Convert( JObject raw ) => throw new NotImplementedException();

        public override JContainer RawData() => _mapInfo.TileMapInfo();
    }
}
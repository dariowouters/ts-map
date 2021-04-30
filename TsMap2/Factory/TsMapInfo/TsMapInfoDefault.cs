using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.TsMapInfo {
    public class TsMapInfoDefault : IFactory, ITsMapInfoFile {
        public string GetFileName() => ITsMapInfoFile.FileName;

        public string GetSavingPath() => throw new NotImplementedException();

        public JObject JsonData() => throw new NotImplementedException();

        public JObject TileMapInfo( int    gameCode,
                                    string gameName,
                                    string gameVersion,
                                    int    mapPadding,
                                    int    tileSize,
                                    float  x1,
                                    float  x2,
                                    float  y1,
                                    float  y2,
                                    int    minZoom,
                                    int    maxZoom ) =>
            throw new NotImplementedException();
    }
}
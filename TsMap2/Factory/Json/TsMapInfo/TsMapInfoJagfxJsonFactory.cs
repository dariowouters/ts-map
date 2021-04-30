using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json.TsMapInfo {
    public class TsMapInfoJagfxJsonFactory : Factory, ITsMapInfoJsonFactory {
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

        public override string GetFileName() => ITsMapInfoJsonFactory.Filename;

        public override string GetSavingPath() => throw new NotImplementedException();

        public override JObject JsonData() => throw new NotImplementedException();
    }
}
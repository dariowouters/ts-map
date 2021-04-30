using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.TsMapInfo {
    public interface ITsMapInfoFile {
        public const string FileName = "TileMapInfo.json";

        JObject TileMapInfo( int    gameCode,
                             string gameName,
                             string gameVersion,
                             int    mapPadding,
                             int    tileSize,
                             float  x1,
                             float  x2,
                             float  y1,
                             float  y2,
                             int    minZoom,
                             int    maxZoom );
    }
}
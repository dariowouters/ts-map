using Newtonsoft.Json.Linq;

namespace TsMap.TsTileMapInfo {
    public interface ITsTileMapInfo {
        JObject TileMapInfo( TsGame game,
                             int    mapPadding,
                             int    tileSize,
                             float  x1,
                             float  x2,
                             float  y1,
                             float  y2,
                             int    minZoom,
                             int    maxZoom,
                             string gameVersion );
    }
}
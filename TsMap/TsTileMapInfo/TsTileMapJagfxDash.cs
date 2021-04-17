using System;
using Newtonsoft.Json.Linq;

namespace TsMap.TsTileMapInfo {
    public class TsTileMapJagfxDash : ITsTileMapInfo {
        public static readonly string Name = "JAGFx/ets2-dashboard-skin";

        public JObject TileMapInfo( TsGame game,
                                    int    mapPadding,
                                    int    tileSize,
                                    float  x1,
                                    float  x2,
                                    float  y1,
                                    float  y2,
                                    int    minZoom,
                                    int    maxZoom,
                                    string gameVersion ) =>
            new JObject {
                [ "map" ] = new JObject {
                    [ "maxX" ]     = tileSize * 256,
                    [ "maxY" ]     = tileSize * 256,
                    [ "x1" ]       = x1,
                    [ "x2" ]       = x2,
                    [ "y1" ]       = y1,
                    [ "y2" ]       = y2,
                    [ "tileSize" ] = tileSize,
                    [ "minZoom" ]  = minZoom,
                    [ "maxZoom" ]  = maxZoom
                },
                [ "game" ] = new JObject {
                    [ "id" ]          = game.code,
                    [ "game" ]        = game.code,
                    [ "name" ]        = game.FullName(),
                    [ "version" ]     = gameVersion,
                    [ "generatedAt" ] = DateTime.Now
                }
            };
    }
}
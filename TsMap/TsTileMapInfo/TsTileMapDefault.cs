using System;
using Newtonsoft.Json.Linq;

namespace TsMap.TsTileMapInfo {
    public class TsTileMapDefault : ITsTileMapInfo {
        public static readonly string Name = "Default";

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
                [ "maxX" ]        = tileSize * 256,
                [ "maxY" ]        = tileSize * 256,
                [ "x1" ]          = x1,
                [ "x2" ]          = x2,
                [ "y1" ]          = y1,
                [ "y2" ]          = y2,
                [ "tileSize" ]    = tileSize,
                [ "minZoom" ]     = minZoom,
                [ "maxZoom" ]     = maxZoom,
                [ "gameCode" ]    = game.code,
                [ "gameName" ]    = game.FullName(),
                [ "gameVersion" ] = gameVersion,
                [ "generatedAt" ] = DateTime.Now
            };
    }
}
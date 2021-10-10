using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Model.TsMapInfo {
    public class TsMapInfoJagfx : TsMapInfo {
        public TsMapInfoJagfx( TsGame game, int mapPadding, int tileSize, float x1, float x2, float y1, float y2, int minZoom, int maxZoom ) :
            base( game, mapPadding, tileSize, x1, x2, y1, y2, minZoom, maxZoom ) { }

        public override JObject TileMapInfo() =>
            new JObject {
                [ "map" ] = new JObject {
                    [ "maxX" ]     = _tileSize * 256,
                    [ "maxY" ]     = _tileSize * 256,
                    [ "x1" ]       = _x1,
                    [ "x2" ]       = _x2,
                    [ "y1" ]       = _y1,
                    [ "y2" ]       = _y2,
                    [ "tileSize" ] = _tileSize,
                    [ "minZoom" ]  = _minZoom,
                    [ "maxZoom" ]  = _maxZoom
                },
                [ "game" ] = new JObject {
                    [ "id" ]          = _game.Code,
                    [ "game" ]        = _game.Code,
                    [ "name" ]        = _game.FullName(),
                    [ "version" ]     = _game.Version,
                    [ "generatedAt" ] = DateTime.Now
                }
            };
    }
}
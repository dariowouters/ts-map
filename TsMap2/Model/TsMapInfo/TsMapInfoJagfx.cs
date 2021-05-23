using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Model.TsMapInfo {
    public class TsMapInfoJagfx : TsMapInfo {
        public TsMapInfoJagfx( TsGame game, int mapPadding, int tileSize, float x1, float x2, float y1, float y2, int minZoom, int maxZoom ) :
            base( game, mapPadding, tileSize, x1, x2, y1, y2, minZoom, maxZoom ) { }

        public override JObject TileMapInfo() =>
            new JObject {
                [ "map" ] = new JObject {
                    [ "maxX" ]     = this._tileSize * 256,
                    [ "maxY" ]     = this._tileSize * 256,
                    [ "x1" ]       = this._x1,
                    [ "x2" ]       = this._x2,
                    [ "y1" ]       = this._y1,
                    [ "y2" ]       = this._y2,
                    [ "tileSize" ] = this._tileSize,
                    [ "minZoom" ]  = this._minZoom,
                    [ "maxZoom" ]  = this._maxZoom
                },
                [ "game" ] = new JObject {
                    [ "id" ]          = this._game.Code,
                    [ "game" ]        = this._game.Code,
                    [ "name" ]        = this._game.FullName(),
                    [ "version" ]     = this._game.Version,
                    [ "generatedAt" ] = DateTime.Now
                }
            };
    }
}
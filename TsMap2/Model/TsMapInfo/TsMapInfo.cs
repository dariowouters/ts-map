using System;
using Newtonsoft.Json.Linq;

namespace TsMap2.Model.TsMapInfo {
    public abstract class TsMapInfo {
        protected readonly TsGame _game;
        protected readonly int    _maxZoom;
        protected readonly int    _minZoom;
        protected readonly int    _tileSize;
        protected readonly float  _x1;
        protected readonly float  _x2;
        protected readonly float  _y1;
        protected readonly float  _y2;
        protected          int    _mapPadding;

        public TsMapInfo( TsGame game, int mapPadding, int tileSize, float x1, float x2, float y1, float y2, int minZoom, int maxZoom ) {
            this._game       = game;
            this._mapPadding = mapPadding;
            this._tileSize   = tileSize;
            this._x1         = x1;
            this._x2         = x2;
            this._y1         = y1;
            this._y2         = y2;
            this._minZoom    = minZoom;
            this._maxZoom    = maxZoom;
        }

        public virtual JObject TileMapInfo() =>
            new JObject {
                [ "maxX" ]        = this._tileSize * 256,
                [ "maxY" ]        = this._tileSize * 256,
                [ "x1" ]          = this._x1,
                [ "x2" ]          = this._x2,
                [ "y1" ]          = this._y1,
                [ "y2" ]          = this._y2,
                [ "tileSize" ]    = this._tileSize,
                [ "minZoom" ]     = this._minZoom,
                [ "maxZoom" ]     = this._maxZoom,
                [ "gameCode" ]    = this._game.Code,
                [ "gameName" ]    = this._game.FullName(),
                [ "gameVersion" ] = this._game.Version,
                [ "generatedAt" ] = DateTime.Now
            };
    }
}
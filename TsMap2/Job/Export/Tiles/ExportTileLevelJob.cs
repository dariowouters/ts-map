using System;
using System.Drawing;
using Serilog;
using TsMap2.Factory.Picture;
using TsMap2.Helper.Map;
using TsMap2.Model;

namespace TsMap2.Job.Export.Tiles {
    public class ExportTileLevelJob : ThreadJob {
        private readonly int _zoomLevel;

        // private readonly MapRenderer _mapRenderer = new MapRenderer();
        private float  _movement;
        private PointF _oldPos = new PointF( 0, 0 );

        public ExportTileLevelJob( int zoomLevel ) => _zoomLevel = zoomLevel;

        private Settings _settings => Store().Settings;

        public override string JobName() => $"{base.JobName()}_{_zoomLevel}";

        protected override void Do() {
            Log.Information( "[Job][TileLevel] Exporting tile for zoom {0}", _zoomLevel );

            if ( _zoomLevel == 0 ) {
                MapHelper.ZoomOutAndCenterMap( _settings.ExportSettings.TileSize,
                                               _settings.ExportSettings.TileSize,
                                               out PointF pos,
                                               out float zoom );

                SaveTileImage( 0, 0, 0, zoom, pos );
            } else {
                MapHelper.ZoomOutAndCenterMap( (int) Math.Pow( 2, _zoomLevel ) * _settings.ExportSettings.TileSize,
                                               (int) Math.Pow( 2, _zoomLevel ) * _settings.ExportSettings.TileSize,
                                               out PointF pos,
                                               out float zoom );

                // SaveTileImage( _zoomLevel, 15, 13, zoom, pos );
                for ( var x = 0; x < Math.Pow( 2, _zoomLevel ); x++ )
                for ( var y = 0; y < Math.Pow( 2, _zoomLevel ); y++ ) {
                    SaveTileImage( _zoomLevel, x, y, zoom, pos );

                    _oldPos = pos;
                }
            }

            Log.Information( "[Job][TileLevel] End of exporting {0} level", _zoomLevel );
        }

        // z = zoomLevel; x = row tile index; y = column tile index
        private void SaveTileImage( int z, int x, int y, float zoom, PointF pos ) {
            using var bitmap = new Bitmap( _settings.ExportSettings.TileSize,
                                           _settings.ExportSettings.TileSize );
            // using Graphics g = Graphics.FromImage( bitmap );

            pos.X = x == 0
                        ? pos.X
                        : pos.X + bitmap.Width / zoom * x; // get tile start coords
            pos.Y = y == 0
                        ? pos.Y
                        : pos.Y + bitmap.Height / zoom * y;

            if ( z == _settings.ExportSettings.TileZoomMax && _oldPos.Y != 0 && _oldPos.X != 0 && _movement == 0 ) {
                float difference = Math.Abs( _oldPos.Y - pos.Y );
                _movement = difference / _settings.ExportSettings.TileSize;

                Log.Information( "{0}, scale: {1}", difference, difference / _settings.ExportSettings.TileSize );
            }

            var mapRenderer = new MapRenderer( bitmap );
            mapRenderer.Render( zoom, pos );


            var tileFactory = new TileFactory( bitmap );
            tileFactory.Save( z, x, y );
            // Directory.CreateDirectory( $"{exportPath}/Tiles/{z}/{x}" );
            // bitmap.Save( $"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png );
        }
    }
}
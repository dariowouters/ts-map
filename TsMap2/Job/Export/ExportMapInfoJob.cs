using Serilog;
using SixLabors.ImageSharp;
using TsMap2.Factory.Json;
using TsMap2.Helper;
using TsMap2.Model.TsMapInfo;

namespace TsMap2.Job.Export {
    public class ExportMapInfoJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][MapInfo] Exporting..." );

            MapHelper.ZoomOutAndCenterMap( this.Store().Settings.ExportSettings.TileSize,
                                           this.Store().Settings.ExportSettings.TileSize, out PointF pos,
                                           out float zoom );

            var mapInfoJagfx = new TsMapInfoJagfx( this.Store().Game,
                                                   this.Store().Settings.ExportSettings.TilePadding,
                                                   this.Store().Settings.ExportSettings.TileSize,
                                                   pos.X,
                                                   pos.X + this.Store().Settings.ExportSettings.TileSize / zoom,
                                                   pos.Y,
                                                   pos.Y + this.Store().Settings.ExportSettings.TileSize / zoom,
                                                   this.Store().Settings.ExportSettings.TileZoomMin,
                                                   this.Store().Settings.ExportSettings.TileZoomMax );

            var factory = new TsMapInfoJsonFactory( mapInfoJagfx );
            factory.Save();

            Log.Information( "[Job][MapInfo] Saved !" );
        }
    }
}
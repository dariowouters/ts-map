using System.Drawing;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Helper.Map;
using TsMap2.Model.TsMapInfo;

namespace TsMap2.Job.Export {
    public class ExportMapInfoJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][MapInfo] Exporting..." );

            MapHelper.ZoomOutAndCenterMap( Store().Settings.ExportSettings.TileSize,
                                           Store().Settings.ExportSettings.TileSize, out PointF pos,
                                           out float zoom );

            var mapInfoJagfx = new TsMapInfoJagfx( Store().Game,
                                                   Store().Settings.ExportSettings.TilePadding,
                                                   Store().Settings.ExportSettings.TileSize,
                                                   pos.X,
                                                   pos.X + Store().Settings.ExportSettings.TileSize / zoom,
                                                   pos.Y,
                                                   pos.Y + Store().Settings.ExportSettings.TileSize / zoom,
                                                   Store().Settings.ExportSettings.TileZoomMin,
                                                   Store().Settings.ExportSettings.TileZoomMax );

            var factory = new TsMapInfoJsonFactory( mapInfoJagfx );
            factory.Save();

            Log.Information( "[Job][MapInfo] Saved !" );
        }
    }
}
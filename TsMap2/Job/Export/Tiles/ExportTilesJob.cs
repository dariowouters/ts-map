using Serilog;

namespace TsMap2.Job.Export.Tiles {
    public class ExportTilesJob : ParentThreadJob {
        protected override void Do() {
            Store().Map.Prefabs.ForEach( p => p.UpdateLook() );
            Store().Map.Companies.ForEach( p => p.UpdatePrefabItem() );

            int zoomMin = Store().Settings.ExportSettings.TileZoomMin;
            int zoomMax = Store().Settings.ExportSettings.TileZoomMax;

            Log.Debug( "[Job][Export][Tiles] Export tiles from {0} to {1} zoom", zoomMin, zoomMax );

            // for ( int i = zoomMin; i <= zoomMax; i++ ) AddJob( new ExportTileLevelJob( i ) );
            for ( int i = zoomMin; i <= zoomMax; i++ ) new ExportTileLevelJob( i ).RunAndWait();
        }
    }
}
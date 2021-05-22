using Serilog;
using TsMap2.Factory.Json;

namespace TsMap2.Job.Export {
    public class ExportOverlaysJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][ExportOverlays] Exporting..." );
            Log.Debug( "[Job][ExportOverlays] Companies : {0}", this.Store().Map.Overlays.Company.Count );
            Log.Debug( "[Job][ExportOverlays] Parking : {0}",   this.Store().Map.Overlays.Parking.Count );

            // Fixme: Overlays wasn't exported properly. The process seem to be stopped before the end of writing on file.
            var mapFactory = new TsOverlaysJsonFactory( this.Store().Map.Overlays );
            mapFactory.Save();

            Log.Information( "[Job][ExportOverlays] Saved !" );
        }
    }
}
using System.Drawing;
using Serilog;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlaysOthersJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][OverlayOther] Parsing..." );

            foreach ( TsMapMapOverlayItem overlay in this.Store().Map.MapOverlays ) {
                Bitmap b = overlay.Overlay?.GetBitmap();

                if ( !overlay.Valid || overlay.Hidden || b == null ) continue;

                var ov = new TsMapOverlayItem( overlay.X, overlay.Z, overlay.OverlayName, overlay.ZoomLevelVisibility, TsMapOverlayType.Overlay, b );
                this.Store().Map.Overlays.Overlay.Add( ov );
            }

            Log.Information( "[Job][OverlayOther] Others: {0}", this.Store().Map.Overlays.Overlay.Count );
        }
    }
}
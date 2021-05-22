using System.Drawing;
using Serilog;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlayTriggerJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][OverlayTrigger] Parsing..." );

            foreach ( TsMapTriggerItem trigger in this.Store().Map.Triggers ) {
                Bitmap b = trigger.Overlay?.GetBitmap();

                if ( !trigger.Valid || trigger.Hidden || b == null ) continue;

                var ov = new TsMapOverlayItem( trigger.X, trigger.Z, trigger.OverlayName, TsMapOverlayType.Parking, b );
                this.Store().Map.Overlays.Parking.Add( ov );
            }

            Log.Information( "[Job][OverlayTrigger] Loaded: {0}", this.Store().Map.Overlays.Parking.Count );
        }
    }
}
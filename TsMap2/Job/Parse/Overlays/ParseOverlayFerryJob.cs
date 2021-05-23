using System.Drawing;
using Serilog;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlayFerryJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][OverlayFerry] Parsing..." );

            foreach ( TsMapFerryItem ferry in this.Store().Map.FerryConnections ) {
                Bitmap b = ferry.Overlay?.GetBitmap();

                if ( !ferry.Valid || ferry.Hidden || b == null ) continue;
                string overlayName = ScsHash.TokenToString( ferry.OverlayToken );

                if ( ferry.Train ) {
                    var ov = new TsMapOverlayItem( ferry.X, ferry.Z, overlayName, TsMapOverlayType.Train, b );
                    this.Store().Map.Overlays.Train.Add( ov );
                } else {
                    var ov = new TsMapOverlayItem( ferry.X, ferry.Z, overlayName, TsMapOverlayType.Ferry, b );
                    this.Store().Map.Overlays.Ferry.Add( ov );
                }
            }

            Log.Information( "[Job][OverlayFerry] Train: {0}", this.Store().Map.Overlays.Train.Count );
            Log.Information( "[Job][OverlayFerry] Ferry: {0}", this.Store().Map.Overlays.Ferry.Count );
        }
    }
}
using Serilog;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefOverlaysJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][MapOverlay] Loading" );

            var overlayEntry = new ScsOverlayEntry();
            Store().Def.Overlays = overlayEntry.List();

            Log.Information( "[Job][MapOverlay] Loaded. Found: {0}", Store().Def.Overlays.Count );
        }
    }
}
using Serilog;
using TsMap2.Factory.Binaries;

namespace TsMap2.Job.Export {
    public class ExportBinariesJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Export][Binaries] Exporting" );

            // var citiesBinaryFactory = new TsCitiesBinaryFactory( cities );
            // citiesBinaryFactory.Save();

            var pointBinaryFactory = new PointBinaryFactory( Store().Map.Roads );
            pointBinaryFactory.Save();

            Log.Information( "[Job][Export][Binaries] Done" );
        }
    }
}
using Serilog;
using TsMap2.Factory.Json;

namespace TsMap2.Job.Export {
    public class ExportDataOverviewJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][ExportDataOverview] Exporting..." );

            var overviewFactory = new DataOverviewJsonFactory();
            overviewFactory.Save();

            Log.Information( "[Job][ExportDataOverview] Done" );
        }
    }
}
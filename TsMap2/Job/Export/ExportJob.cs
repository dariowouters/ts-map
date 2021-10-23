namespace TsMap2.Job.Export {
    public class ExportJob : ParentThreadJob {
        protected override void Do() {
            AddJob( new ExportMapInfoJob() );
            AddJob( new ExportCitiesJob() );
            AddJob( new ExportCountriesJob() );
            AddJob( new ExportOverlaysJob() );
            AddJob( new ExportGeoJsonJob() );
            // AddJob( new ExportBinariesJob() );
        }
    }
}
namespace TsMap2.Job.Export {
    public class ExportJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ExportMapInfoJob() );
            this.AddJob( new ExportCitiesJob() );
            this.AddJob( new ExportCountriesJob() );
            this.AddJob( new ExportOverlaysJob() );
        }
    }
}
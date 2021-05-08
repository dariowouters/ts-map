namespace TsMap2.Job.Export {
    public class ExportJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ExportCitiesJob() );
        }

        protected override void OnEnd() { }
    }
}
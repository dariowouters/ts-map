namespace TsMap2.Job.Parse {
    public class ParseScsFilesJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseGameJob() );
            this.AddJob( new ParseCountryFilesJob() );
            this.AddJob( new ParsePrefabFilesJob() );
        }

        protected override void OnEnd() { }
    }
}
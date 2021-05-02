namespace TsMap2.Job.Parse {
    public class ParseScsFilesJob : ParentThreadJob {
        public override string JobName() => "ParseScsFilesJob";

        protected override void Do() {
            this.AddJob( new ParseGameJob() );
            this.AddJob( new ParseCountryFilesJob() );
            this.AddJob( new ParsePrefabFilesJob() );
        }

        protected override void OnEnd() { }
    }
}
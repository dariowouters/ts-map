namespace TsMap2.Job.Parse {
    public class ParseScsMapJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseMapFilesJob() );
        }

        protected override void OnEnd() { }
    }
}
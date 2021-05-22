namespace TsMap2.Job.Parse.Map {
    public class ParseMapJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseMapFilesJob() );
            this.AddJob( new ParseMapLocalizationsJob() );
        }

        protected override void OnEnd() { }
    }
}
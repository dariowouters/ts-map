namespace TsMap2.Job.Parse.Map {
    public class ParseMapJob : ParentThreadJob {
        protected override void Do() {
            AddJob( new ParseMapFilesJob() );
            AddJob( new ParseMapLocalizationsJob() );
        }
    }
}
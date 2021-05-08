namespace TsMap2.Job.Parse.Def {
    public class ParseScsDefFilesJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseGameJob() );
            this.AddJob( new ParseCityFilesJob() );
            this.AddJob( new ParseCountryFilesJob() );
            this.AddJob( new ParseRoadLookFilesJob() );
            this.AddJob( new ParsePrefabFilesJob() );
            this.AddJob( new ParseFerryConnectionsFilesJob() );
            this.AddJob( new ParseOverlaysJob() );
        }

        protected override void OnEnd() { }
    }
}
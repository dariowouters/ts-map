namespace TsMap2.Job.Parse {
    public class ParseScsDefFilesJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseGameJob() );
            this.AddJob( new ParseCityFilesJob() );
            this.AddJob( new ParseCountryFilesJob() );
            this.AddJob( new ParseRoadLookFilesJob() );
            this.AddJob( new ParsePrefabFilesJob() );

            // FIXME This may occur crash ?!
            this.AddJob( new ParseFerryConnectionsFilesJob() );
        }

        protected override void OnEnd() { }
    }
}
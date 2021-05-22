namespace TsMap2.Job.Parse.Def {
    public class ParseScsDefJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseGameJob() );
            this.AddJob( new ParseDefCitiesJob() );
            this.AddJob( new ParseDefCountriesJob() );
            this.AddJob( new ParseDefRoadLooksJob() );
            this.AddJob( new ParseDefPrefabsJob() );
            this.AddJob( new ParseDefFerryConnectionsJob() );
            this.AddJob( new ParseDefOverlaysJob() );
        }
    }
}
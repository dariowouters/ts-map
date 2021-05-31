namespace TsMap2.Job.Parse.Def {
    public class ParseScsDefJob : ParentThreadJob {
        protected override void Do() {
            AddJob( new ParseGameJob() );
            AddJob( new ParseDefCitiesJob() );
            AddJob( new ParseDefCountriesJob() );
            AddJob( new ParseDefPrefabsJob() );
            AddJob( new ParseDefFerryConnectionsJob() );
            AddJob( new ParseDefOverlaysJob() );
        }
    }
}
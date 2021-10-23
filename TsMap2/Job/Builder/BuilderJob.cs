namespace TsMap2.Job.Builder {
    public class BuilderJob : ParentThreadJob {
        protected override void Do() {
            AddJob( new BuilderCitiesJob() );
            AddJob( new BuilderRoadsJob() );
        }
    }
}
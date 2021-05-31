namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlaysJob : ParentThreadJob {
        protected override void Do() {
            AddJob( new ParseOverlaysOthersJob() );
            AddJob( new ParseOverlayCompanyJob() );
            AddJob( new ParseOverlayTriggerJob() );
            AddJob( new ParseOverlayFerryJob() );
            AddJob( new ParseOverlayPrefabJob() );
        }
    }
}
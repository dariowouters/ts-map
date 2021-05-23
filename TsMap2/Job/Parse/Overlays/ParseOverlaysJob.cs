namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlaysJob : ParentThreadJob {
        protected override void Do() {
            this.AddJob( new ParseOverlayCompanyJob() );
            this.AddJob( new ParseOverlayTriggerJob() );
            this.AddJob( new ParseOverlayFerryJob() );
        }
    }
}
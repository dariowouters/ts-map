namespace TsMap2.Job.Export.Tiles {
    public class ExportTilesJob : ParentThreadJob {
        protected override void Do() {
            for ( var i = 5; i > 0; i-- ) AddJob( new ExportTileLevelJob( i ) );
        }
    }
}
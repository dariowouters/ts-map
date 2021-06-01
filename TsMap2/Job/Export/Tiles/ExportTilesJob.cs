namespace TsMap2.Job.Export.Tiles {
    public class ExportTilesJob : ParentThreadJob {
        protected override void Do() {
            for ( var i = 3; i > 0; i-- ) AddJob( new ExportTileLevelJob( i ) );
        }
    }
}
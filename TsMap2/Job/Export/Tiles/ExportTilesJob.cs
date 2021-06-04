namespace TsMap2.Job.Export.Tiles {
    public class ExportTilesJob : ParentThreadJob {
        protected override void Do() {
            Store().Map.Prefabs.ForEach( p => p.UpdateLook() );
            Store().Map.Companies.ForEach( p => p.UpdatePrefabItem() );

            for ( var i = 0; i <= 5; i++ ) AddJob( new ExportTileLevelJob( i ) );
            // for ( var i = 1; i <= 5; i++ ) new ExportTileLevelJob( i ).RunAndWait();
            // new ExportTileLevelJob( 5 ).RunAndWait();
        }
    }
}
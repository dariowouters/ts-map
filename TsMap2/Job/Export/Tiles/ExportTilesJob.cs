namespace TsMap2.Job.Export.Tiles {
    public class ExportTilesJob : ParentThreadJob {
        protected override void Do() {
            Store().Map.Prefabs.ForEach( p => p.UpdateLook() );
            Store().Map.Companies.ForEach( p => p.UpdatePrefabItem() );

            for ( var i = 5; i > 0; i-- ) AddJob( new ExportTileLevelJob( i ) );
        }
    }
}
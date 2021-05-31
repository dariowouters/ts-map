namespace TsMap2.Model {
    public class Export {
        public TsExportType ExportType  = TsExportType.JAGFX;
        public int          TilePadding = 384;
        public int          TileSize    = 512;
        public int          TileZoomMax = 8;
        public int          TileZoomMin;

        public Export() { }

        public Export( int tileZoomMin, int tileZoomMax, int tileSize, int tilePadding, TsExportType exportType ) {
            TileZoomMin = tileZoomMin;
            TileZoomMax = tileZoomMax;
            TileSize    = tileSize;
            TilePadding = tilePadding;
            ExportType  = exportType;
        }
    }

    public enum TsExportType {
        DEFAULT,
        JAGFX
    }
}
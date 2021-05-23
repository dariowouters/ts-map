namespace TsMap2.Model {
    public class Export {
        public TsExportType ExportType  = TsExportType.JAGFX;
        public int          TilePadding = 384;
        public int          TileSize    = 512;
        public int          TileZoomMax = 8;
        public int          TileZoomMin;

        public Export() { }

        public Export( int tileZoomMin, int tileZoomMax, int tileSize, int tilePadding, TsExportType exportType ) {
            this.TileZoomMin = tileZoomMin;
            this.TileZoomMax = tileZoomMax;
            this.TileSize    = tileSize;
            this.TilePadding = tilePadding;
            this.ExportType  = exportType;
        }
    }

    public enum TsExportType {
        DEFAULT,
        JAGFX
    }
}
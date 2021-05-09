using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCutPlaneItem : TsMapItem {
        public TsMapCutPlaneItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = false;
            this.TsCutPlaneItem825( startOffset );
        }

        public void TsCutPlaneItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int nodeCount  = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 );
            fileOffset     += 0x04       + 0x08 * nodeCount;
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}
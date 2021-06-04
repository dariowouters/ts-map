using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsMapCutPlaneItem : TsMapItem {
        public ScsMapCutPlaneItem( ScsSector sector ) : base( sector, sector.LastOffset ) {
            Valid = false;
            TsCutPlaneItem825();
        }

        private void TsCutPlaneItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int nodeCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 );
            fileOffset += 0x04       + 0x08 * nodeCount;
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}
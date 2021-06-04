using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsMapFuelPumpItem : TsMapItem {
        public ScsMapFuelPumpItem( ScsSector sector ) : base( sector, sector.LastOffset ) {
            Valid = false;
            if ( Sector.Version < 855 )
                TsFuelPumpItem825();
            else if ( Sector.Version >= 855 )
                TsFuelPumpItem855();
            else
                Log.Warning(
                            $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        private void TsFuelPumpItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05       + 0x10;           // 0x05(flags) + 0x10(node_uid & prefab_uid)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsFuelPumpItem855() {
            int fileOffset      = Sector.LastOffset + 0x34;                                           // Set position at start of flags
            int subItemUidCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 + 0x10 ); // 0x05(flags) + 0x10(2 uids)
            fileOffset += 0x04       + 0x08 * subItemUidCount;                                        // 0x04(subItemUidCount) + subItemUids
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}
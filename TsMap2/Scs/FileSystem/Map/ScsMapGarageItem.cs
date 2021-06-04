using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsMapGarageItem : TsMapItem {
        public ScsMapGarageItem( ScsSector sector ) : base( sector, sector.LastOffset ) {
            Valid = false;
            // int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            if ( Sector.Version < 855 )
                TsGarageItem825();
            else if ( Sector.Version >= 855 )
                TsGarageItem855();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        private void TsGarageItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05       + 0x1C;           // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsGarageItem855() {
            int fileOffset      = Sector.LastOffset + 0x34;                                           // Set position at start of flags
            int subItemUidCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 + 0x1C ); // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            fileOffset += 0x04       + 0x08 * subItemUidCount;                                        // 0x04(subItemUidCount) + subItemUids
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}
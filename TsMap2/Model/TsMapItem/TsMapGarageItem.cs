using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsMapGarageItem : TsMapItem {
        public TsMapGarageItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            Valid = false;
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            if ( Sector.Version < 855 )
                TsGarageItem825( startOffset );
            else if ( Sector.Version >= 855 )
                TsGarageItem855( startOffset );
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsGarageItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05       + 0x1C;     // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            BlockSize  =  fileOffset - startOffset;
        }

        public void TsGarageItem855( int startOffset ) {
            int fileOffset      = startOffset + 0x34;                                                 // Set position at start of flags
            int subItemUidCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 + 0x1C ); // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            fileOffset += 0x04       + 0x08 * subItemUidCount;                                        // 0x04(subItemUidCount) + subItemUids
            BlockSize  =  fileOffset - startOffset;
        }
    }
}
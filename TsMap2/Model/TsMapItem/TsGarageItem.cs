using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsGarageItem : TsItem {
        public TsGarageItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = false;
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            if ( this.Sector.Version < 855 )
                this.TsGarageItem825( startOffset );
            else if ( this.Sector.Version >= 855 )
                this.TsGarageItem855( startOffset );
            else
                Log.Warning(
                            $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsGarageItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset     += 0x05       + 0x1C; // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsGarageItem855( int startOffset ) {
            int fileOffset      = startOffset + 0x34;                                                      // Set position at start of flags
            int subItemUidCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 + 0x1C ); // 0x05(flags) + 0x1C(city_name & m_type & 2 uids)
            fileOffset     += 0x04       + 0x08 * subItemUidCount;                                         // 0x04(subItemUidCount) + subItemUids
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}
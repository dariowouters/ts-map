using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsMapTrafficRuleItem : TsMapItem {
        public TsMapTrafficRuleItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            Valid = false;
            if ( Sector.Version < 834 )
                TsTrafficRuleItem825( startOffset );
            else if ( Sector.Version >= 834 )
                TsTrafficRuleItem834( startOffset );
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsTrafficRuleItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34;                                          // Set position at start of flags
            int nodeCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            fileOffset += 0x04       + 0x08 * nodeCount + 0x0C;                           // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            BlockSize  =  fileOffset - startOffset;
        }

        public void TsTrafficRuleItem834( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int tagCount   = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            int nodeCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount ); // 0x04(tagCount) + tags;
            fileOffset += 0x04       + 0x08 * nodeCount + 0x0C; // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            BlockSize  =  fileOffset - startOffset;
        }
    }
}
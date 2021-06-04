using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsMapTrafficRuleItem : TsMapItem {
        public ScsMapTrafficRuleItem( ScsSector sector ) : base( sector, sector.LastOffset ) {
            Valid = false;
            if ( Sector.Version < 834 )
                TsTrafficRuleItem825();
            else if ( Sector.Version >= 834 )
                TsTrafficRuleItem834();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        private void TsTrafficRuleItem825() {
            int fileOffset = Sector.LastOffset + 0x34;                                    // Set position at start of flags
            int nodeCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            fileOffset += 0x04       + 0x08 * nodeCount + 0x0C;                           // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsTrafficRuleItem834() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int tagCount   = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            int nodeCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount ); // 0x04(tagCount) + tags;
            fileOffset += 0x04       + 0x08 * nodeCount + 0x0C; // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}
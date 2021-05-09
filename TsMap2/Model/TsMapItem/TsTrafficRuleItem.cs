using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsTrafficRuleItem : TsItem {
        public TsTrafficRuleItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = false;
            if ( this.Sector.Version < 834 )
                this.TsTrafficRuleItem825( startOffset );
            else if ( this.Sector.Version >= 834 )
                this.TsTrafficRuleItem834( startOffset );
            else
                Log.Warning(
                            $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsTrafficRuleItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34;                                               // Set position at start of flags
            int nodeCount  = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            fileOffset     += 0x04       + 0x08 * nodeCount + 0x0C;                            // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsTrafficRuleItem834( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int tagCount   = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            int nodeCount  = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount ); // 0x04(tagCount) + tags;
            fileOffset     += 0x04       + 0x08 * nodeCount + 0x0C; // 0x04(nodeCount) + nodeUids + 0x0C(traffic_rule_id & range)
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}
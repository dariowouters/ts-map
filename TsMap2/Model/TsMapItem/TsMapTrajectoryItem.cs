using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsMapTrajectoryItem : TsMapItem {
        public TsMapTrajectoryItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = false;
            if ( this.Sector.Version < 846 )
                this.TsTrajectoryItem834( startOffset );
            else if ( this.Sector.Version >= 846 )
                this.TsTrajectoryItem846( startOffset );
            else
                Log.Warning( $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsTrajectoryItem834( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags

            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            int accessRuleCount =
                MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * nodeCount + 0x08 ); // 0x04(nodeCount) + nodeUids + 0x08(flags2 & count1)
            int routeRuleCount  = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * accessRuleCount ); // 0x04(accessRuleCount) + accessRules
            int checkpointCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x14 * routeRuleCount ); // 0x04(routeRuleCount) + routeRules
            fileOffset     += 0x04       + 0x10 * checkpointCount + 0x04; // 0x04(checkpointCount) + checkpoints + 0x04(padding2)
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsTrajectoryItem846( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags

            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            int routeRuleCount =
                MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * nodeCount + 0x0C ); // 0x04(nodeCount) + nodeUids + 0x0C(flags2 & access_rule)
            int checkpointCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x1C * routeRuleCount ); // 0x04(routeRuleCount) + routeRules
            fileOffset     += 0x04       + 0x10 * checkpointCount + 0x04; // 0x04(checkpointCount) + checkpoints + 0x04(padding2)
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}
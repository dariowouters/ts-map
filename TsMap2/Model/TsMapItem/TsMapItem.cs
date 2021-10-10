using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapItem {
        protected const int VegetationSphereBlockSize    = 0x14;
        protected const int VegetationSphereBlockSize825 = 0x10;

        protected readonly ScsSector   Sector;
        public readonly    ScsItemType Type;

        public readonly ulong  Uid;
        public readonly float  X;
        public readonly float  Z;
        protected       TsNode EndNode;
        protected       ulong  EndNodeUid;
        protected       TsNode StartNode;
        protected       ulong  StartNodeUid;

        public TsMapItem( ScsSector sector, int offset ) {
            Sector = sector;

            int fileOffset = offset;

            Type = (ScsItemType) MemoryHelper.ReadUInt32( Sector.Stream, fileOffset );
            Uid  = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x04 );
            X    = MemoryHelper.ReadSingle( Sector.Stream, fileOffset += 0x08 );
            Z    = MemoryHelper.ReadSingle( Sector.Stream, fileOffset += 0x08 );
        }

        public List< ulong > Nodes     { get; protected set; }
        public int           BlockSize { get; protected set; }
        public bool          Valid     { get; protected set; }
        public bool          Hidden    { get; protected set; }

        public TsNode GetStartNode() {
            return StartNode ??= Store().Map.GetNodeByUid( StartNodeUid );
        }

        public TsNode GetEndNode() {
            return EndNode ??= Store().Map.GetNodeByUid( EndNodeUid );
        }

        protected static StoreHelper Store() => StoreHelper.Instance;
    }
}
using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsItem {
        protected const int VegetationSphereBlockSize    = 0x14;
        protected const int VegetationSphereBlockSize825 = 0x10;

        protected readonly TsSector Sector;
        protected          TsNode   EndNode;
        protected          ulong    EndNodeUid;
        protected          TsNode   StartNode;
        protected          ulong    StartNodeUid;

        public TsItem( TsSector sector, int offset ) {
            this.Sector = sector;

            int fileOffset = offset;

            this.Type = (TsItemType) MemoryHelper.ReadUInt32( this.Sector.Stream, fileOffset );

            this.Uid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x04 );

            this.X = MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x08 );
            this.Z = MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x08 );
        }

        public ulong Uid { get; }

        public List< ulong > Nodes { get; protected set; }

        public int BlockSize { get; protected set; }

        public bool Valid { get; protected set; }

        public TsItemType Type   { get; }
        public float      X      { get; }
        public float      Z      { get; }
        public bool       Hidden { get; protected set; }

        public TsNode GetStartNode() {
            return this.StartNode ??= Store().Map.GetNodeByUid( this.StartNodeUid );
        }

        public TsNode GetEndNode() {
            return this.EndNode ??= Store().Map.GetNodeByUid( this.EndNodeUid );
        }

        protected static StoreHelper Store() => StoreHelper.Instance;
    }
}
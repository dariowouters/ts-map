using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsItem {
        public const int    VegetationSphereBlockSize    = 0x14;
        public const int    VegetationSphereBlockSize825 = 0x10;
        protected    TsNode EndNode;
        protected    ulong  EndNodeUid;
        protected    TsNode StartNode;
        protected    ulong  StartNodeUid;

        public TsItem( ulong startNodeUid, ulong endNodeUid, ulong uid, int blockSize, bool valid, TsItemType type, float x, float z, bool hidden ) {
            this.StartNodeUid = startNodeUid;
            this.EndNodeUid   = endNodeUid;
            this.Uid          = uid;
            this.BlockSize    = blockSize;
            this.Valid        = valid;
            this.Type         = type;
            this.X            = x;
            this.Z            = z;
            this.Hidden       = hidden;
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
            return this.StartNode ??= _store().Map.GetNodeByUid( this.StartNodeUid );
        }

        public TsNode GetEndNode() {
            return this.EndNode ??= _store().Map.GetNodeByUid( this.EndNodeUid );
        }

        private static StoreHelper _store() => StoreHelper.Instance;
    }
}
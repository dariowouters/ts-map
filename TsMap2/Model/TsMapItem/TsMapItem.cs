using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapItem {
        protected const int VegetationSphereBlockSize    = 0x14;
        protected const int VegetationSphereBlockSize825 = 0x10;

        protected readonly ScsSector Sector;
        protected          TsNode    EndNode;
        protected          ulong     EndNodeUid;
        protected          TsNode    StartNode;
        protected          ulong     StartNodeUid;

        public TsMapItem( ScsSector sector ) => Sector = sector;

        public ulong         Uid       => Sector.Uid;
        public List< ulong > Nodes     { get; protected set; }
        public int           BlockSize { get; protected set; }
        public bool          Valid     { get; protected set; }
        public ScsItemType   Type      => Sector.ItemType;
        public float         X         => Sector.X;
        public float         Z         => Sector.Z;
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
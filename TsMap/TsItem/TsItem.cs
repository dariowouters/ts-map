using System.Collections.Generic;

namespace TsMap.TsItem
{
    public class TsItem
    {
        protected const int VegetationSphereBlockSize = 0x14;
        protected const int VegetationSphereBlockSize825 = 0x10;

        protected readonly TsSector Sector;
        public ulong Uid { get; }
        protected ulong StartNodeUid;
        protected ulong EndNodeUid;
        protected TsNode StartNode;
        protected TsNode EndNode;

        public List<ulong> Nodes { get; protected set; }

        public int BlockSize { get; protected set; }

        public bool Valid { get; protected set; }

        public TsItemType Type { get; }
        public float X { get; }
        public float Z { get; }
        public bool Hidden { get; protected set; }

        public TsItem(TsSector sector, int offset)
        {
            Sector = sector;

            var fileOffset = offset;

            Type = (TsItemType)MemoryHelper.ReadUInt32(Sector.Stream, fileOffset);

            Uid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x04);

            X = MemoryHelper.ReadSingle(Sector.Stream, fileOffset += 0x08);
            Z = MemoryHelper.ReadSingle(Sector.Stream, fileOffset += 0x08);
        }

        public TsNode GetStartNode()
        {
            return StartNode ?? (StartNode = Sector.Mapper.GetNodeByUid(StartNodeUid));
        }

        public TsNode GetEndNode()
        {
            return EndNode ?? (EndNode = Sector.Mapper.GetNodeByUid(EndNodeUid));
        }

    }
}

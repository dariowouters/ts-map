using System;
using TsMap.Helpers;

namespace TsMap
{
    public class TsNode
    {
        public ulong Uid { get; }

        public float X { get; }
        public float Z { get; }
        public float Rotation { get; }

        public ulong BackwardItemUid { get; }
        public ulong ForwardItemUid { get; }
        public TsItem.TsItem BackwardItem { get { return _mapper.GetItemByUid(BackwardItemUid); } }
        public TsItem.TsItem ForwardItem { get { return _mapper.GetItemByUid(ForwardItemUid); } }

        public TsCountry ForwardNodeCountry { get; }
        public TsCountry BackwardNodeCountry { get; }

        private TsMapper _mapper;

        public TsNode(TsSector sector, int fileOffset)
        {
            Uid = MemoryHelper.ReadUInt64(sector.Stream, fileOffset);
            X = MemoryHelper.ReadInt32(sector.Stream, fileOffset += 0x08) / 256f;
            Z = MemoryHelper.ReadInt32(sector.Stream, fileOffset += 0x08) / 256f;

            var rX = MemoryHelper.ReadSingle(sector.Stream, fileOffset += 0x04);
            var rZ = MemoryHelper.ReadSingle(sector.Stream, fileOffset + 0x08);

            BackwardItemUid = MemoryHelper.ReadUInt64(sector.Stream, fileOffset += 0x08);
            ForwardItemUid = MemoryHelper.ReadUInt64(sector.Stream, fileOffset += 0x08);

            var rot = Math.PI - Math.Atan2(rZ, rX);
            Rotation = (float) (rot % Math.PI * 2);

            ForwardNodeCountry = sector.Mapper.GetCountryById(MemoryHelper.ReadInt8(sector.Stream, fileOffset += 0x09));
            BackwardNodeCountry = sector.Mapper.GetCountryById(MemoryHelper.ReadInt8(sector.Stream, fileOffset += 0x01));

            _mapper = sector.Mapper;
        }

        public TsCountry GetCountry()
        {
            if (ForwardNodeCountry != null) return ForwardNodeCountry;
            return BackwardNodeCountry;
        }
    }
}

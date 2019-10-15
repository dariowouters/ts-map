using System.Collections.Generic;
using System.IO;

namespace TsMap.TsItem
{
    public class TsMapAreaItem : TsItem
    {
        public List<ulong> NodeUids { get; private set; }
        public uint ColorIndex { get; private set; }
        public bool DrawOver { get; private set; }

        public TsMapAreaItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsMapAreaItem825(startOffset);
        }

        public void TsMapAreaItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            DrawOver = MemoryHelper.ReadUint8(Sector.Stream, fileOffset) != 0;
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;

            NodeUids = new List<ulong>();

            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            fileOffset += 0x04; // 0x04(nodeCount)
            for (var i = 0; i < nodeCount; i++)
            {
                NodeUids.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            ColorIndex = MemoryHelper.ReadUInt32(Sector.Stream, fileOffset);
            fileOffset += 0x04; // 0x04(colorIndex)
            BlockSize = fileOffset - startOffset;
        }
    }
}

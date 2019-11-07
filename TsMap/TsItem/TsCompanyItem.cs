using System.Collections.Generic;
using System.IO;
using TsMap.HashFiles;

namespace TsMap.TsItem
{
    public class TsCompanyItem : TsItem
    {
        public ulong OverlayToken { get; private set; }
        public TsMapOverlay Overlay { get; private set; }

        public TsCompanyItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            Nodes = new List<ulong>();
            if (Sector.Version < 858)
                TsCompanyItem825(startOffset);
            else if (Sector.Version >= 858)
                TsCompanyItem858(startOffset);
            else
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsCompanyItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;

            OverlayToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            Overlay = Sector.Mapper.LookupOverlay(OverlayToken);
            if (Overlay == null)
            {
                Valid = false;
                if (OverlayToken != 0)
                    Log.Msg(
                        $"Could not find Company Overlay: '{ScsHash.TokenToString(OverlayToken)}'({OverlayToken:X}), in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08)); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x08); // count | 0x08 (uid[1] & uid[2])
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count2
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count3
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count4
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count5
            fileOffset += 0x04 + (0x08 * count);
            BlockSize = fileOffset - startOffset;
        }
        public void TsCompanyItem858(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;

            OverlayToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            Overlay = Sector.Mapper.LookupOverlay(OverlayToken);
            if (Overlay == null)
            {
                Valid = false;
                if (OverlayToken != 0)
                    Log.Msg(
                        $"Could not find Company Overlay: '{ScsHash.TokenToString(OverlayToken)}'({OverlayToken:X}), in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }

            Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08)); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x08); // count | 0x08 (uid[1] & uid[2])
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count2
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count3
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count4
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count5
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count6
            fileOffset += 0x04 + (0x08 * count);
            BlockSize = fileOffset - startOffset;
        }
    }
}

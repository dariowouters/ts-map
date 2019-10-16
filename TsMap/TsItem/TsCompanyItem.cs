using System.IO;

namespace TsMap.TsItem
{
    public class TsCompanyItem : TsItem
    {
        public TsMapOverlay Overlay { get; private set; }

        public TsCompanyItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
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

            var overlayId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            Overlay = Sector.Mapper.LookupOverlay(overlayId);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayId != 0) Log.Msg($"Could not find Company Overlay with id: {overlayId:X}, in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x20); // count
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

            var overlayId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            Overlay = Sector.Mapper.LookupOverlay(overlayId);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayId != 0) Log.Msg($"Could not find Company Overlay with id: {overlayId:X}, in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x20); // count
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

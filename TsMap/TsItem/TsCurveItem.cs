using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsCurveItem : TsItem
    {
        public TsCurveItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version < 907)
                TsCurveItem896(startOffset);
            else if (Sector.Version >= 907)
                TsCurveItem907(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                                      $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsCurveItem896(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var heightOffsetCount =
                MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05 + 0x6C); // 0x05(flags) + 0x6C(offset to heightOffsetCount)
            fileOffset +=
                0x04 + 0x04 * heightOffsetCount; // 0x04(heightOffsetCount) + heightOffsets
            BlockSize = fileOffset - startOffset;
        }

        public void TsCurveItem907(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var subCurvesCount =
                MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05 + (4 * 0x08) + 0x04); // 0x05(flags) + 4 * 0x08(4 node uids) + 0x04(length)
            fileOffset += 0x04; // 0x04(subCurvesCount)

            for (int i = 0; i < subCurvesCount; i++)
            {
                var heightOffsetCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x4c); // 0x4C(offset to heightOffsetCount)
                fileOffset += 0x04 + 0x04 * heightOffsetCount + 0x14; // 0x04(heightOffsetCount) + heightOffsets + 0x14(to end of subcurve struct)
            }

            BlockSize = fileOffset - startOffset;
        }
    }
}

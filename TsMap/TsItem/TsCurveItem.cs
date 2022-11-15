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

            if (Sector.Version >= 896)
                TsCurveItem896(startOffset);
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
    }
}

using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsBuildingItem : TsItem
    {
        public TsBuildingItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version >= 858)
                TsBuildingItem858(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                                      $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsBuildingItem858(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var buildingHeightOffsetCount =
                MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05 + 0x2C); // 0x05(flags) + 0x2C(offset to buildingHeightOffsetCount)
            fileOffset +=
                0x04 + 0x04 * buildingHeightOffsetCount; // 0x04(buildingHeightOffsetCount) + buildingHeightOffsets
            BlockSize = fileOffset - startOffset;
        }
    }
}

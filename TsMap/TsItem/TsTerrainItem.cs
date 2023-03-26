using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;
using TsMap.TsItem.Shared;

namespace TsMap.TsItem
{
    public class TsTerrainItem : TsItem
    {
        public TsTerrainItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version >= 884)
                TsTerrainItem884(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                                      $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsTerrainItem884(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var vegSphereCount =
                MemoryHelper.ReadInt32(Sector.Stream,
                    fileOffset += 0x05 + 0xEA); // 0x05(flags) + 0xE6(offset to veg sphere count)
            fileOffset += 0x04 + VegetationSphereBlockSize * vegSphereCount; // 0x04(vegSphereCount) + vegSpheres
            fileOffset += QuadInfo.Parse(Sector, fileOffset); // quad info 1
            fileOffset += QuadInfo.Parse(Sector, fileOffset); // quad info 2
            fileOffset += 0x20; // 0x20(right_edge + right_edge_look + left_edge + left_edge_look)
            BlockSize = fileOffset - startOffset;
        }
    }
}
